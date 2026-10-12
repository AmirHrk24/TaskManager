using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Abstractions;

// Read side (Dapper, raw SQL, no tracking)

public interface ITaskQueries
{
    Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <param name="scopeTeamId">When set, only tasks of this team are returned (TeamManager scope).</param>
    /// <param name="scopeAssigneeId">When set, only tasks assigned to this user are returned (Employee scope).</param>
    Task<PagedResult<TaskDto>> GetPagedAsync(TaskFilter filter, int? scopeTeamId, int? scopeAssigneeId, CancellationToken ct = default);
}

public interface ITeamQueries
{
    Task<TeamDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TeamDto>> GetAllAsync(int? scopeTeamId, CancellationToken ct = default);
}

public interface IUserQueries
{
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(int? scopeTeamId, CancellationToken ct = default);
}
