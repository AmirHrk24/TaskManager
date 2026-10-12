using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public sealed class AuthService(
    IUnitOfWork uow,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request,CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await uow.FirstOrDefaultAsync<User>(
            u => u.Email == email, ct);

        // Same message for every failure -> no user enumeration.
        if (user is null ||!user.IsActive ||!hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = jwt.Generate(user);

        return new LoginResponse
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            TeamId = user.TeamId
        };
    }
}