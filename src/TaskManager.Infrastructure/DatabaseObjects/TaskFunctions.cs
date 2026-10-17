
namespace TaskManager.Infrastructure.DatabaseObjects;

internal static class TaskFunctions
{
    public const string CreateTaskCountByTeam = """
        CREATE OR ALTER FUNCTION dbo.fn_TaskCountByTeam
        (
            @TeamId INT
        )
        RETURNS INT
        AS
        BEGIN
            DECLARE @TaskCount INT;

            SELECT @TaskCount = COUNT(*)
            FROM dbo.Tasks
            WHERE TeamId = @TeamId;

            RETURN @TaskCount;
        END;
        """;
}
