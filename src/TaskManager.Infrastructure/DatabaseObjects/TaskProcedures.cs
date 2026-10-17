namespace TaskManager.Infrastructure.DatabaseObjects;

internal static class TaskProcedures
{
    public const string CreateTeamTaskReport = """
        CREATE OR ALTER PROCEDURE dbo.sp_GetTeamTaskReport
            @TeamId INT
        AS
        BEGIN
            SET NOCOUNT ON;

            ;WITH TeamTasks AS
            (
                SELECT
                    t.Id AS TaskId,
                    t.TeamId,
                    t.Title,
                    t.Status,
                    t.Priority
                FROM dbo.Tasks AS t
                WHERE t.TeamId = @TeamId
            )
            SELECT
                TaskId,
                TeamId,
                Title,
                Status,
                Priority
            INTO #TeamTaskReport
            FROM TeamTasks;

            SELECT
                TeamId,
                COUNT(*) AS TotalTasks,
                SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END)
                    AS CompletedTasks,
                SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END)
                    AS TodoTasks,
                SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END)
                    AS InProgressTasks,
                SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END)
                    AS InReviewTasks
            FROM #TeamTaskReport
            GROUP BY TeamId;

            DROP TABLE #TeamTaskReport;
        END;
        """;
    public const string CreateTeamTaskDetailsReport = """
    CREATE OR ALTER PROCEDURE dbo.sp_GetTeamTaskDetailsReport
        @TeamId INT
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            t.Id AS TaskId,
            t.TeamId,
            t.Title,
            t.Status,
            t.Priority
        FROM dbo.Tasks AS t
        WHERE t.TeamId = @TeamId;
    END;
    """;
}