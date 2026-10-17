
namespace TaskManager.Infrastructure.DatabaseObjects;

internal static class TaskViews
{
    public const string CreateOrAlterTaskSummary = """
        CREATE OR ALTER VIEW dbo.vw_TaskSummary
        AS
        SELECT
            t.Id AS TaskId,
            t.TeamId,
            t.AssignedToUserId,
            t.CreatedByUserId
        FROM dbo.Tasks AS t;
        """;
}
