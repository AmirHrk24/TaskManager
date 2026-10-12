using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Services;

namespace TaskManager.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskAccessPolicy, TaskAccessPolicy>();
        services.AddScoped<ITaskStatusPolicy, TaskStatusPolicy>();
        services.AddScoped<IManagementAccessPolicy, ManagementAccessPolicy>();

        return services;
    }
}