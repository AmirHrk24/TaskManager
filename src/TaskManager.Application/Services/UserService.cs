using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public sealed class UserService(
    IUserQueries queries,
    IPasswordHasher hasher,
    IUnitOfWork uow,
    ICurrentUser currentUser,
    IManagementAccessPolicy accessPolicy) : IUserService
{
    public async Task<UserDto> CreateAsync(CreateUserRequest request,CancellationToken ct = default)
    {
        accessPolicy.EnsureAdmin();

        var email = request.Email.Trim().ToLowerInvariant();

        if (await uow.AnyAsync<User>(
                user => user.Email == email, ct))
        {
            throw new BadRequestException(
                "A user with this email already exists.");
        }

        if (request.TeamId.HasValue &&
            await uow.GetByIdAsync<Team>([request.TeamId.Value], ct) is null)
        {
            throw new NotFoundException("Team not found.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            Role = request.Role,
            TeamId = request.TeamId
        };

        uow.Add(user);
        await uow.SaveChangesAsync(ct);

        return (await queries.GetByIdAsync(user.Id, ct))!;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var scopeTeamId = accessPolicy.ResolveTeamScopeForList();

        return await queries.GetAllAsync(scopeTeamId, ct);
    }

    public async Task<UserDto> GetCurrentAsync(CancellationToken ct = default)
    {
        return await queries.GetByIdAsync(currentUser.UserId, ct)
            ?? throw new NotFoundException("User not found.");
    }
}