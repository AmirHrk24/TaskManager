
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Abstractions;

public interface ITaskSummaryEfQuery
{
    Task<IReadOnlyList<TaskSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
