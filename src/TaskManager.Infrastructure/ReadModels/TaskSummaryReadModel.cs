namespace TaskManager.Infrastructure.ReadModels;

public sealed class TaskSummaryReadModel
{
    public int TaskId { get; set; }

    public int TeamId { get; set; }

    public int? AssignedToUserId { get; set; }

    public int CreatedByUserId { get; set; }
}