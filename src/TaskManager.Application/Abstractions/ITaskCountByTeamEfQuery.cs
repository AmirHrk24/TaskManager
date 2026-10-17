
namespace TaskManager.Application.Abstractions;

public interface ITaskCountByTeamEfQuery
{
    Task<int> GetCountAsync(int teamId,CancellationToken cancellationToken = default);
}
