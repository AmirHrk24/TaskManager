using TaskManager.Application.DTOs;

namespace TaskManager.Application.Abstractions;

public interface ITaskOverviewService
{
    Task<IReadOnlyList<TaskOverviewDto>> GetAllAsync(CancellationToken ct = default);
}