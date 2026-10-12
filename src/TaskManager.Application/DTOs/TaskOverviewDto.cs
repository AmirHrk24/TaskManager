namespace TaskManager.Application.DTOs;

public sealed class TaskOverviewDto
{
    public int TaskId { get; init; }

    public string TaskTitle { get; init; } = string.Empty;

    public int UserId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public int TeamId { get; init; }

    public string TeamName { get; init; } = string.Empty;
}