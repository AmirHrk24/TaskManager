using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Queries.EfQuery;

public sealed class TaskSummaryEfQuery(AppDbContext dbContext)
    : ITaskSummaryEfQuery
{
    public async Task<IReadOnlyList<TaskManager.Application.DTOs.TaskSummaryDto>>
        GetAllAsync(CancellationToken cancellationToken = default)
    {
        var summaries = await dbContext.TaskSummaries
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return summaries
            .Select(summary => new TaskManager.Application.DTOs.TaskSummaryDto
            {
                TaskId = summary.TaskId,
                TeamId = summary.TeamId,
                AssignedToUserId = summary.AssignedToUserId,
                CreatedByUserId = summary.CreatedByUserId
            })
            .ToList();
    }
}