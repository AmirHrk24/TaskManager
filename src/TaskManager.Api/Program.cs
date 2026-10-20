using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
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

var builder = WebApplication.CreateBuilder(args);

// Elasticsearch + Serilog

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

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(
            new[] { new Uri(elasticUrl) },
            options =>
            {
                options.DataStream =
                    new DataStreamName("logs", "taskmanager-api", "default");

                options.BootstrapMethod = BootstrapMethod.Silent;
            },
            transport =>
            {
                transport.Authentication(
                    new BasicAuthentication(
                        elasticUsername,
                        elasticPassword));

                transport.CertificateFingerprint(caCertificateFingerprint);
            });
});

// ---------- Services (Dependency Injection) ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI document + Bearer (JWT) security scheme -> enables the "Authorize" button in Swagger UI.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });

        return Task.CompletedTask;
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ITaskStatusPolicy, TaskStatusPolicy>();

builder.Services.AddSingleton<FileRequestLogWriter>(
    _ => new FileRequestLogWriter(Path.Combine(builder.Environment.ContentRootPath, "Logs")));

builder.Services.AddSingleton<IRequestLogWriter, CompositeRequestLogWriter>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Authentication (JWT) ----------
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? throw new InvalidOperationException("The 'Jwt' configuration section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep claim names as written in the token ("sub", "role", ...)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

// ---------- Authorization ----------
// Every endpoint requires a logged-in user unless it is marked [AllowAnonymous].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

var app = builder.Build();

// ---------- Middleware pipeline (order matters) ----------
app.UseMiddleware<RequestResponseLoggingMiddleware>(); // outermost: logs everything, including errors/401/403
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous(); // /openapi/v1.json
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "TaskManager API v1");
        o.EnablePersistAuthorization(); // keep the JWT after a page refresh
    }); // /swagger
    await app.Services.InitializeDatabaseAsync(); // migrate + seed demo data
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
