
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Services;

public sealed class TaskDetailsEfQuery : ITaskDetailsEfQuery
{
    private readonly AppDbContext _dbContext;
    private readonly ITaskAccessPolicy _accessPolicy;

    public TaskDetailsEfQuery(
        AppDbContext dbContext,
        ITaskAccessPolicy accessPolicy)
    {
        _dbContext = dbContext;
        _accessPolicy = accessPolicy;
    }

    public async Task<TaskDetailsDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var task = await _dbContext.Tasks
            .AsNoTracking()
            .Include(t => t.Team)
                .ThenInclude(team => team.Manager)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (task is null)
        {
            return null;
        }

        _accessPolicy.EnsureCanAccess(
            task.TeamId,
            task.AssignedToUserId);

        return new TaskDetailsDto
        {
            TaskId = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,

            TeamId = task.TeamId,
            TeamName = task.Team.Name,

            TeamManagerId = task.Team.Manager?.Id,
            TeamManagerName = task.Team.Manager?.FullName,

            AssignedToUserId = task.AssignedToUserId,
            AssignedToName = task.AssignedTo?.FullName,

            CreatedByUserId = task.CreatedByUserId,
            CreatedByName = task.CreatedBy.FullName
        };
    }
}
