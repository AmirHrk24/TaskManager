using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.ReadModels;

namespace TaskManager.Infrastructure.Queries.EfQuery;

public sealed class TeamTaskReportEfQuery(AppDbContext dbContext)
    : ITeamTaskReportEfQuery
{
    public async Task<IReadOnlyList<TeamTaskReportDto>> GetByTeamAsync(
        int teamId,
        CancellationToken cancellationToken = default)
    {
        var reports = await dbContext
            .Set<TeamTaskReportReadModel>()
            .FromSqlInterpolated(
                $"EXEC dbo.sp_GetTeamTaskDetailsReport @TeamId = {teamId}")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return reports.Select(report => new TeamTaskReportDto
        {
            TaskId = report.TaskId,
            TeamId = report.TeamId,
            Title = report.Title,
            Status = report.Status,
            Priority = report.Priority
        }).ToList();
    }
}