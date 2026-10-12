
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Abstractions;

public interface ITaskDetailsEfQuery
{
    Task<TaskDetailsDto?> GetByIdAsync(int id,CancellationToken ct = default);
}
