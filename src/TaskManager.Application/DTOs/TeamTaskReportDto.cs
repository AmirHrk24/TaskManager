namespace TaskManager.Application.DTOs;

public sealed class TeamTaskReportDto
{
    public int TaskId { get; set; }

    public int TeamId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Status { get; set; }

    public int Priority { get; set; }
}