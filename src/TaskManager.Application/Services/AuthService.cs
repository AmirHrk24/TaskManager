
using Microsoft.Extensions.Logging;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public sealed class AuthService(
    IUnitOfWork uow,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt,
    ITokenStore tokenStore,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await uow.FirstOrDefaultAsync<User>(
            u => u.Email == email,
            ct);

        if (user is null ||
            !user.IsActive ||
            !hasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning(
                "Authentication failed. EventType: {EventType}",
                "LoginFailure");

            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var token = jwt.Generate(user);

        var ttl = token.ExpiresAtUtc - DateTime.UtcNow;

        await tokenStore.StoreAsync(
            token.Jti,
            user.Id.ToString(),
            ttl,
            ct);

        logger.LogInformation(
            "Authentication succeeded. EventType: {EventType}, UserId: {UserId}, Role: {Role}",
            "LoginSuccess",
            user.Id,
            user.Role);

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
