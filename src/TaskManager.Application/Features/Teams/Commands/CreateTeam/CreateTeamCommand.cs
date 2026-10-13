
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Teams.Commands.CreateTeam;

public sealed record CreateTeamCommand(CreateTeamRequest Request)
    : IRequest<TeamDto>;
