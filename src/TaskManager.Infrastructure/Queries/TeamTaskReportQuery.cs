using Dapper;

namespace TaskManager.Infrastructure.Queries;

public sealed class TeamTaskReportQuery(
    IDbConnectionFactory connectionFactory)
{
    public async Task<IReadOnlyList<TeamTaskReportDto>> GetByTeamAsync(
        int teamId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            EXEC dbo.sp_GetTeamTaskReport @TeamId = @TeamId;
            """;

        using var connection = connectionFactory.Create();

        var command = new CommandDefinition(
            sql,
            new { TeamId = teamId },
            cancellationToken: cancellationToken);

        var result =
            await connection.QueryAsync<TeamTaskReportDto>(command);

        return result.AsList();
    }
}

public sealed record TeamTaskReportDto(
    int TeamId,
    int TotalTasks,
    int CompletedTasks,
    int TodoTasks,
    int InProgressTasks,
    int InReviewTasks);