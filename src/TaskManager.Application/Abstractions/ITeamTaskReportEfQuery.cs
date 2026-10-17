
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Abstractions;

public interface ITeamTaskReportEfQuery
{
    Task<IReadOnlyList<TeamTaskReportDto>> GetByTeamAsync(int teamId,CancellationToken cancellationToken = default);
}
