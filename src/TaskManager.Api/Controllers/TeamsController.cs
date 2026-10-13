
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Teams.Commands.CreateTeam;
using TaskManager.Application.Features.Teams.Commands.SetTeamManager;
using TaskManager.Application.Features.Teams.Queries.GetAllTeams;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public sealed class TeamsController(ISender sender) : ControllerBase
{
    /// <summary>Admin: all teams. TeamManager: his own team.</summary>
    [HttpGet]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll(
        CancellationToken ct)
    {
        var teams = await sender.Send(new GetAllTeamsQuery(), ct);
        return Ok(teams);
    }

    /// <summary>Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<TeamDto>> Create(
        CreateTeamRequest request,
        CancellationToken ct)
    {
        var team = await sender.Send(new CreateTeamCommand(request), ct);

        return CreatedAtAction(nameof(GetAll), null, team);
    }

    /// <summary>Admin only: assign a TeamManager user as the manager of the team.</summary>
    [HttpPut("{id:int}/manager")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<TeamDto>> SetManager(
        int id,
        SetTeamManagerRequest request,
        CancellationToken ct)
    {
        var team = await sender.Send(
            new SetTeamManagerCommand(id, request),
            ct);

        return Ok(team);
    }
}
