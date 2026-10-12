using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Services;

public sealed class TaskOverviewService : ITaskOverviewService
{
    private readonly AppDbContext _dbContext;

    public TaskOverviewService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IReadOnlyList<TaskOverviewDto>> GetAllAsync(
    CancellationToken ct = default)
    {
        var result = await (
            from task in _dbContext.Tasks
            join team in _dbContext.Teams
                on task.TeamId equals team.Id
            join user in _dbContext.Users
                on task.AssignedToUserId equals user.Id
            select new TaskOverviewDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                UserId = user.Id,
                UserName = user.FullName,
                TeamId = team.Id,
                TeamName = team.Name
            }
        ).ToListAsync(ct);

        return result;
    }
}