using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public sealed record CreateTaskRequest
{
    [Required, StringLength(200)]
    public string Title { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [EnumDataType(typeof(TaskPriority))]
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;

    public DateTime? DueDate { get; init; }

    /// <summary>Required for Admin. Ignored/validated against own team for TeamManager.</summary>
    public int? TeamId { get; init; }

    public int? AssignedToUserId { get; init; }
}

public sealed record UpdateTaskRequest
{
    [Required, StringLength(200)]
    public string Title { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [EnumDataType(typeof(TaskPriority))]
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;

    public DateTime? DueDate { get; init; }

    public int? AssignedToUserId { get; init; }
}

public sealed record UpdateTaskStatusRequest
{
    [EnumDataType(typeof(TaskItemStatus))]
    public TaskItemStatus Status { get; init; }
}

public sealed record TaskFilter
{
    public TaskItemStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public int? AssignedToUserId { get; init; }
    public int? TeamId { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
