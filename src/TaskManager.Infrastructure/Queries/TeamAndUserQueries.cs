using Dapper;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;

namespace TaskManager.Infrastructure.Queries;

internal sealed class TeamQueries(IDbConnectionFactory factory) : ITeamQueries
{
    private const string SelectSql = @"
SELECT t.Id, t.Name, t.Description, t.ManagerId, m.FullName AS ManagerName,
       (SELECT COUNT(1) FROM Users u WHERE u.TeamId = t.Id) AS MembersCount
FROM Teams t
LEFT JOIN Users m ON m.Id = t.ManagerId";

    public async Task<TeamDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var connection = factory.Create();
        return await connection.QuerySingleOrDefaultAsync<TeamDto>(
            new CommandDefinition(SelectSql + " WHERE t.Id = @Id", new { Id = id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<TeamDto>> GetAllAsync(int? scopeTeamId, CancellationToken ct = default)
    {
        var sql = SelectSql
                  + (scopeTeamId.HasValue ? " WHERE t.Id = @ScopeTeamId" : string.Empty)
                  + " ORDER BY t.Name";

        using var connection = factory.Create();
        var rows = await connection.QueryAsync<TeamDto>(
            new CommandDefinition(sql, new { ScopeTeamId = scopeTeamId }, cancellationToken: ct));
        return rows.ToList();
    }
}

internal sealed class UserQueries(IDbConnectionFactory factory) : IUserQueries
{
    private const string SelectSql = @"
SELECT u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.TeamId, t.Name AS TeamName, u.CreatedAt
FROM Users u
LEFT JOIN Teams t ON t.Id = u.TeamId";

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var connection = factory.Create();
        return await connection.QuerySingleOrDefaultAsync<UserDto>(
            new CommandDefinition(SelectSql + " WHERE u.Id = @Id", new { Id = id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(int? scopeTeamId, CancellationToken ct = default)
    {
        var sql = SelectSql
                  + (scopeTeamId.HasValue ? " WHERE u.TeamId = @ScopeTeamId" : string.Empty)
                  + " ORDER BY u.FullName";

        using var connection = factory.Create();
        var rows = await connection.QueryAsync<UserDto>(
            new CommandDefinition(sql, new { ScopeTeamId = scopeTeamId }, cancellationToken: ct));
        return rows.ToList();
    }
}
