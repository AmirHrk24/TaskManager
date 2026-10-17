
using Dapper;

namespace TaskManager.Infrastructure.Queries;

public sealed class TaskSummaryQuery(IDbConnectionFactory connectionFactory)
{
    public async Task<IReadOnlyList<TaskSummaryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                TaskId,
                TeamId,
                AssignedToUserId,
                CreatedByUserId
            FROM dbo.vw_TaskSummary
            ORDER BY TaskId;
            """;

        using var connection = connectionFactory.Create();

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<TaskSummaryDto>(command);

        return result.AsList();
    }
}

public sealed record TaskSummaryDto(
    int TaskId,
    int? TeamId,
    int? AssignedToUserId,
    int CreatedByUserId);
