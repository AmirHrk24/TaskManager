using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public sealed record TokenResult(
    string Token,
    DateTime ExpiresAtUtc,
    string Jti);

public interface IJwtTokenGenerator
{
    TokenResult Generate(User user);
}