using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Abstractions;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AuthSessionController(
    ITokenStore tokenStore) : ControllerBase
{
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken ct)
    {
        var jti = User.FindFirst("jti")?.Value;

        if (string.IsNullOrWhiteSpace(jti))
        {
            return Unauthorized();
        }

        await tokenStore.RevokeAsync(jti, ct);

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }
}