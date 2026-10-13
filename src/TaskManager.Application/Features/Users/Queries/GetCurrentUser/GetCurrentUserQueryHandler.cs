
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    IUserQueries queries,
    ICurrentUser currentUser)
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        return await queries.GetByIdAsync(
                   currentUser.UserId,
                   cancellationToken)
               ?? throw new NotFoundException("User not found.");
    }
}
