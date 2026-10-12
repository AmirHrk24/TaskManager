using System.Security.Claims;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Services;

/// <summary>Reads the authenticated user's data from the JWT claims.</summary>
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int UserId =>
        int.TryParse(Principal?.FindFirst("sub")?.Value, out var id)
            ? id
            : throw new UnauthorizedException();

    public UserRole Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirst("role")?.Value, out var role)
            ? role
            : throw new UnauthorizedException();

    public int? TeamId =>
        int.TryParse(Principal?.FindFirst("team_id")?.Value, out var teamId) ? teamId : null;
}
