
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands.CreateUser;
using TaskManager.Application.Features.Users.Queries.GetAllUsers;
using TaskManager.Application.Features.Users.Queries.GetCurrentUser;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Current logged-in user.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var user = await sender.Send(new GetCurrentUserQuery(), ct);
        return Ok(user);
    }

    /// <summary>Admin: all users. TeamManager: members of his own team.</summary>
    [HttpGet]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        CancellationToken ct)
    {
        var users = await sender.Send(new GetAllUsersQuery(), ct);
        return Ok(users);
    }

    /// <summary>Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        CancellationToken ct)
    {
        var user = await sender.Send(new CreateUserCommand(request), ct);

        return CreatedAtAction(nameof(Me), null, user);
    }
}
