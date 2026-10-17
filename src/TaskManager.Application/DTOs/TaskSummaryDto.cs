
namespace TaskManager.Application.DTOs;

public sealed class TaskSummaryDto
{
    public int TaskId { get; set; }

    public int TeamId { get; set; }

    public int? AssignedToUserId { get; set; }

    public int CreatedByUserId { get; set; }
}
