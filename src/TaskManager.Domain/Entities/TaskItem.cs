using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public TaskItemStatus Status { get; private set; } = TaskItemStatus.Todo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public void ChangeStatus(TaskItemStatus status)
    {
        Status = status;
    }
}
