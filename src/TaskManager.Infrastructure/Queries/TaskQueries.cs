using Dapper;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;

namespace TaskManager.Infrastructure.Queries;

internal sealed class TaskQueries(IDbConnectionFactory factory) : ITaskQueries
{
    private const string SelectSql = @"
SELECT t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
       t.TeamId, tm.Name AS TeamName,
       t.AssignedToUserId, au.FullName AS AssignedToName,
       t.CreatedByUserId, cu.FullName AS CreatedByName,
       t.CreatedAt, t.UpdatedAt
FROM Tasks t
INNER JOIN Teams tm ON tm.Id = t.TeamId
LEFT JOIN Users au ON au.Id = t.AssignedToUserId
INNER JOIN Users cu ON cu.Id = t.CreatedByUserId";

    public async Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var connection = factory.Create();
        var sql = SelectSql + " WHERE t.Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<TaskDto>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<PagedResult<TaskDto>> GetPagedAsync(
        TaskFilter filter, int? scopeTeamId, int? scopeAssigneeId, CancellationToken ct = default)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // Role scope (always applied, cannot be overridden by the client filter)
        if (scopeTeamId.HasValue)
        {
            conditions.Add("t.TeamId = @ScopeTeamId");
            parameters.Add("ScopeTeamId", scopeTeamId.Value);
        }
        if (scopeAssigneeId.HasValue)
        {
            conditions.Add("t.AssignedToUserId = @ScopeAssigneeId");
            parameters.Add("ScopeAssigneeId", scopeAssigneeId.Value);
        }

        // Client filters
        if (filter.TeamId.HasValue)
        {
            conditions.Add("t.TeamId = @TeamId");
            parameters.Add("TeamId", filter.TeamId.Value);
        }
        if (filter.AssignedToUserId.HasValue)
        {
            conditions.Add("t.AssignedToUserId = @AssignedToUserId");
            parameters.Add("AssignedToUserId", filter.AssignedToUserId.Value);
        }
        if (filter.Status.HasValue)
        {
            conditions.Add("t.Status = @Status");
            parameters.Add("Status", (int)filter.Status.Value);
        }
        if (filter.Priority.HasValue)
        {
            conditions.Add("t.Priority = @Priority");
            parameters.Add("Priority", (int)filter.Priority.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            conditions.Add("(t.Title LIKE @Search OR t.Description LIKE @Search)");
            parameters.Add("Search", $"%{filter.Search.Trim()}%");
        }

        var where = conditions.Count > 0 ? " WHERE " + string.Join(" AND ", conditions) : string.Empty;

        parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
        parameters.Add("PageSize", filter.PageSize);

        var sql = $@"
SELECT COUNT(1) FROM Tasks t{where};
{SelectSql}{where}
ORDER BY t.CreatedAt DESC, t.Id DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var connection = factory.Create();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: ct));

        var total = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<TaskDto>()).ToList();

        return new PagedResult<TaskDto>(items, total, filter.Page, filter.PageSize);
    }
}
