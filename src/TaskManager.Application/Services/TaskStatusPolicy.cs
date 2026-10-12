using TaskManager.Application.Abstractions;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class TaskStatusPolicy : ITaskStatusPolicy
{
    public bool CanChange(
        TaskItemStatus currentStatus,
        TaskItemStatus newStatus)
    {
        if (currentStatus == newStatus)
            return true;

        return currentStatus switch
        {
            TaskItemStatus.Todo =>
                newStatus == TaskItemStatus.InProgress,

            TaskItemStatus.InProgress =>
                newStatus == TaskItemStatus.InReview,

            TaskItemStatus.InReview =>
                newStatus == TaskItemStatus.Done,

            TaskItemStatus.Done => false,

            _ => false
        };
    }
}
