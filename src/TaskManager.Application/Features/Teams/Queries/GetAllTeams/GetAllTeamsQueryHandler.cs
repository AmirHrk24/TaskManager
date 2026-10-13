
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Teams.Queries.GetAllTeams;

public sealed class GetAllTeamsQueryHandler(
    ITeamQueries queries,
    IManagementAccessPolicy accessPolicy)
    : IRequestHandler<GetAllTeamsQuery, IReadOnlyList<TeamDto>>
{
    public async Task<IReadOnlyList<TeamDto>> Handle(
        GetAllTeamsQuery request,
        CancellationToken cancellationToken)
    {
        var scopeTeamId = accessPolicy.ResolveTeamScopeForList();

        return await queries.GetAllAsync(
            scopeTeamId,
            cancellationToken);
    }
}
