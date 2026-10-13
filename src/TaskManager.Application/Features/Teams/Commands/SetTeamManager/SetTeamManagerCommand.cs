
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Teams.Commands.SetTeamManager;

public sealed record SetTeamManagerCommand(
    int Id,
    SetTeamManagerRequest Request) : IRequest<TeamDto>;
