
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Teams.Queries.GetAllTeams;

public sealed record GetAllTeamsQuery : IRequest<IReadOnlyList<TeamDto>>;
