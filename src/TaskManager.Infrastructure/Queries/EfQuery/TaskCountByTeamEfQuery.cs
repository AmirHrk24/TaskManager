
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Queries.EfQuery;

public sealed class TaskCountByTeamEfQuery(AppDbContext dbContext)
    : ITaskCountByTeamEfQuery
{
    public async Task<int> GetCountAsync(
        int teamId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Database
            .SqlQuery<int>(
                $"SELECT dbo.fn_TaskCountByTeam({teamId}) AS Value")
            .SingleAsync(cancellationToken);
    }
}
