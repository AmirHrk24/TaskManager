
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Teams.Commands.CreateTeam;

public sealed class CreateTeamCommandHandler(
    ITeamQueries queries,
    IUnitOfWork uow,
    IManagementAccessPolicy accessPolicy)
    : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(
        CreateTeamCommand request,
        CancellationToken cancellationToken)
    {
        accessPolicy.EnsureAdmin();

        var name = request.Request.Name.Trim();

        if (await uow.AnyAsync<Team>(
                team => team.Name == name,
                cancellationToken))
        {
            throw new BadRequestException(
                "A team with this name already exists.");
        }

        var team = new Team
        {
            Name = name,
            Description = request.Request.Description?.Trim()
        };

        uow.Add(team);

        await uow.SaveChangesAsync(cancellationToken);

        return (await queries.GetByIdAsync(
            team.Id,
            cancellationToken))!;
    }
}
