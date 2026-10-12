using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Auth;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Queries;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core (write side)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(
            sp => sp.GetRequiredService<AppDbContext>());

        // Dapper (read side)
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<ITaskQueries, TaskQueries>();
        services.AddScoped<ITeamQueries, TeamQueries>();
        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<ITaskOverviewService, TaskOverviewService>();
        services.AddScoped<ITaskOverviewEfService, TaskOverviewEfService>();

        // Security
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }

    /// <summary>
    /// Applies pending migrations and seeds demo data.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db, uow, hasher);
    }
}