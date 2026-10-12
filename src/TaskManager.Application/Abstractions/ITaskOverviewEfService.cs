using TaskManager.Application.DTOs;

namespace TaskManager.Application.Abstractions;

public interface ITaskOverviewEfService
{
    Task<IReadOnlyList<TaskOverviewDto>> GetAllAsync(CancellationToken ct = default);
}