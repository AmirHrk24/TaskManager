
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserQueries queries,
    IPasswordHasher hasher,
    IUnitOfWork uow,
    IManagementAccessPolicy accessPolicy)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        accessPolicy.EnsureAdmin();

        var email = request.Request.Email.Trim().ToLowerInvariant();

        if (await uow.AnyAsync<User>(
                user => user.Email == email,
                cancellationToken))
        {
            throw new BadRequestException(
                "A user with this email already exists.");
        }

        if (request.Request.TeamId.HasValue &&
            await uow.GetByIdAsync<Team>(
                [request.Request.TeamId.Value],
                cancellationToken) is null)
        {
            throw new NotFoundException("Team not found.");
        }

        var user = new User
        {
            FullName = request.Request.FullName.Trim(),
            Email = email,
            PasswordHash = hasher.Hash(request.Request.Password),
            Role = request.Request.Role,
            TeamId = request.Request.TeamId
        };

        uow.Add(user);

        await uow.SaveChangesAsync(cancellationToken);

        return (await queries.GetByIdAsync(
            user.Id,
            cancellationToken))!;
    }
}
