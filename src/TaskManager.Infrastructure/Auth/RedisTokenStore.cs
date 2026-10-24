using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using TaskManager.Application.Abstractions;

namespace TaskManager.Infrastructure.Auth;

public sealed class RedisTokenStore(
    IConnectionMultiplexer redis) : ITokenStore
{
    private readonly StackExchange.Redis.IDatabase _database = redis.GetDatabase();

    private static string GetKey(string jti)
        => $"auth:token:{jti}";

    public async Task StoreAsync(
        string jti,
        string userId,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException(
                "Token jti cannot be empty.", nameof(jti));

        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(ttl), "Token TTL must be positive.");

        await _database.StringSetAsync(
            GetKey(jti),
            userId,
            ttl);
    }

    public async Task<bool> ExistsAsync(
        string jti,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(jti))
            return false;

        return await _database.KeyExistsAsync(GetKey(jti));
    }
}