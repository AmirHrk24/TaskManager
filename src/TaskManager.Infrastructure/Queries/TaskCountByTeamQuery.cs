
using Dapper;

namespace TaskManager.Infrastructure.Queries;

public sealed class TaskCountByTeamQuery(
    IDbConnectionFactory connectionFactory)
{
    public async Task<int> GetCountAsync(
        int teamId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT dbo.fn_TaskCountByTeam(@TeamId);
            """;

        using var connection = connectionFactory.Create();

        var command = new CommandDefinition(
            sql,
            new { TeamId = teamId },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<int>(command);
    }
}
