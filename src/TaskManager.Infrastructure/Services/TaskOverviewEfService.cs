using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Services;

public sealed class TaskOverviewEfService : ITaskOverviewEfService
{
    private readonly AppDbContext _dbContext;

    public TaskOverviewEfService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaskOverviewDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var result = await _dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.AssignedToUserId != null)
            .Select(task => new TaskOverviewDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                UserId = task.AssignedTo!.Id,
                UserName = task.AssignedTo.FullName,
                TeamId = task.Team.Id,
                TeamName = task.Team.Name
            })
            .ToListAsync(ct);

        return result;
    }
}