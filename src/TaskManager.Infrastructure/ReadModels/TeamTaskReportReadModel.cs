
namespace TaskManager.Infrastructure.ReadModels;

public sealed class TeamTaskReportReadModel
{
    public int TaskId { get; set; }

    public int TeamId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Status { get; set; }

    public int Priority { get; set; }
}
