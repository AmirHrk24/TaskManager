namespace TaskManager.Application.DTOs;

public sealed class TaskDetailsDto
{
    public int TaskId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string Status { get; init; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public DateTime? DueDate { get; init; }

    public int TeamId { get; init; }

    public string TeamName { get; init; } = string.Empty;

    public int? TeamManagerId { get; init; }

    public string? TeamManagerName { get; init; }

    public int? AssignedToUserId { get; init; }

    public string? AssignedToName { get; init; }

    public int CreatedByUserId { get; init; }

    public string CreatedByName { get; init; } = string.Empty;
}
