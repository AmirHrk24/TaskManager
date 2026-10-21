using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using TaskManager.Api.Logging;
using TaskManager.Api.Middleware;
using TaskManager.Api.Services;
using TaskManager.Application;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Services;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Auth;
using TaskManager.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// ---------- Elasticsearch ----------
var elasticUrl = builder.Configuration["Elasticsearch:Url"]
    ?? throw new InvalidOperationException("Elasticsearch:Url is missing.");

var elasticUsername = builder.Configuration["Elasticsearch:Username"]
    ?? throw new InvalidOperationException("Elasticsearch:Username is missing.");

var elasticPassword = builder.Configuration["Elasticsearch:Password"]
    ?? throw new InvalidOperationException("Elasticsearch:Password is missing.");

var caCertificateFingerprint =
    builder.Configuration["Elasticsearch:CaCertificateFingerprint"]
    ?? throw new InvalidOperationException(
        "Elasticsearch:CaCertificateFingerprint is missing.");

var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
    .Authentication(new BasicAuthentication(elasticUsername, elasticPassword))
    .CertificateFingerprint(caCertificateFingerprint);

builder.Services.AddSingleton(new ElasticsearchClient(settings));

builder.Services.AddSingleton<ElasticsearchLogWriter>();
builder.Services.AddSingleton<FileRequestLogWriter>(
    _ => new FileRequestLogWriter(
        Path.Combine(builder.Environment.ContentRootPath, "Logs")));

builder.Services.AddSingleton<IRequestLogWriter, CompositeRequestLogWriter>();

// ---------- Controllers ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---------- OpenAPI + Bearer ----------
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITaskStatusPolicy, TaskStatusPolicy>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Authentication (JWT) ----------
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "The 'Jwt' configuration section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

// ---------- Authorization ----------
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

var app = builder.Build();

// ---------- Middleware pipeline ----------
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "TaskManager API v1");
        o.EnablePersistAuthorization();
    });

    await app.Services.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
