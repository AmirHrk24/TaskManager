using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto> GetCurrentAsync(CancellationToken ct = default);
}

public interface ITeamService
{
    Task<TeamDto> CreateAsync(CreateTeamRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TeamDto>> GetAllAsync(CancellationToken ct = default);
    Task<TeamDto> SetManagerAsync(int teamId, SetTeamManagerRequest request, CancellationToken ct = default);
}

public interface ITaskService
{
    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<TaskDto>> GetPagedAsync(TaskFilter filter, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> UpdateStatusAsync(int id, UpdateTaskStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
