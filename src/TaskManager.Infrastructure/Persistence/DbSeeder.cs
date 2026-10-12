using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Persistence;

internal static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        IUnitOfWork uow,
        IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync())
            return;

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@taskmanager.com",
            PasswordHash = hasher.Hash("Admin@123"),
            Role = UserRole.Admin
        };

        var team = new Team
        {
            Name = "Development",
            Description = "Software development team"
        };

        db.Users.Add(admin);
        db.Teams.Add(team);

        await uow.SaveChangesAsync();

        var manager = new User
        {
            FullName = "Team Manager",
            Email = "manager@taskmanager.com",
            PasswordHash = hasher.Hash("Manager@123"),
            Role = UserRole.TeamManager,
            TeamId = team.Id
        };

        var employee1 = new User
        {
            FullName = "Employee One",
            Email = "employee1@taskmanager.com",
            PasswordHash = hasher.Hash("Employee@123"),
            Role = UserRole.Employee,
            TeamId = team.Id
        };

        var employee2 = new User
        {
            FullName = "Employee Two",
            Email = "employee2@taskmanager.com",
            PasswordHash = hasher.Hash("Employee@123"),
            Role = UserRole.Employee,
            TeamId = team.Id
        };

        db.Users.AddRange(manager, employee1, employee2);

        await uow.SaveChangesAsync();

        team.ManagerId = manager.Id;

        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "Design database schema",
                Description = "Create the ERD and review it with the team.",
                Priority = TaskPriority.High,
                TeamId = team.Id,
                AssignedToUserId = employee1.Id,
                CreatedByUserId = manager.Id,
                DueDate = DateTime.UtcNow.AddDays(7)
            },
            new TaskItem
            {
                Title = "Implement login page",
                Priority = TaskPriority.Medium,
                TeamId = team.Id,
                AssignedToUserId = employee2.Id,
                CreatedByUserId = manager.Id,
                DueDate = DateTime.UtcNow.AddDays(14)
            });

        await uow.SaveChangesAsync();
    }
}