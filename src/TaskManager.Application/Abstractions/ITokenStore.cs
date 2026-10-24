namespace TaskManager.Application.Abstractions;

public interface ITokenStore
{
    Task StoreAsync(
        string jti,
        string userId,
        TimeSpan ttl,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string jti,
        CancellationToken ct = default);

    Task RevokeAsync(
    string jti,
    CancellationToken ct = default);
}