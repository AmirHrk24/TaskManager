
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(
    IUserQueries queries,
    IManagementAccessPolicy accessPolicy)
    : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var scopeTeamId = accessPolicy.ResolveTeamScopeForList();

        return await queries.GetAllAsync(
            scopeTeamId,
            cancellationToken);
    }
}
