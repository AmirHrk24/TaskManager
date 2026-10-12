
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Abstractions;

public interface ITaskStatusPolicy
{
    bool CanChange(
        TaskItemStatus currentStatus,
        TaskItemStatus newStatus);
}
