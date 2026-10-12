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
            .Select(task => new TaskOverviewDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,

                UserId = task.AssignedToUserId ?? 0,
                UserName = task.AssignedTo != null
                    ? task.AssignedTo.FullName
                    : string.Empty,

                TeamId = task.TeamId,
                TeamName = task.Team.Name
            })
            .ToListAsync(ct);

        return result;
    }
}