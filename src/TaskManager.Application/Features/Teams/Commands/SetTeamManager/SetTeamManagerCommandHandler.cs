
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Features.Teams.Commands.SetTeamManager;

public sealed class SetTeamManagerCommandHandler(
    ITeamQueries queries,
    IUnitOfWork uow,
    IManagementAccessPolicy accessPolicy)
    : IRequestHandler<SetTeamManagerCommand, TeamDto>
{
    public async Task<TeamDto> Handle(
        SetTeamManagerCommand request,
        CancellationToken cancellationToken)
    {
        accessPolicy.EnsureAdmin();

        var team = await uow.GetByIdAsync<Team>(
                       [request.Id],
                       cancellationToken)
                   ?? throw new NotFoundException("Team not found.");

        var user = await uow.GetByIdAsync<User>(
                       [request.Request.ManagerId],
                       cancellationToken)
                   ?? throw new NotFoundException("User not found.");

        if (user.Role != UserRole.TeamManager)
        {
            throw new BadRequestException(
                "The selected user does not have the TeamManager role.");
        }

        if (!user.IsActive)
        {
            throw new BadRequestException(
                "The selected user is inactive.");
        }

        if (user.TeamId.HasValue && user.TeamId.Value != request.Id)
        {
            throw new BadRequestException(
                "The selected user already belongs to another team.");
        }

        user.TeamId = request.Id;
        team.ManagerId = user.Id;

        await uow.SaveChangesAsync(cancellationToken);

        return (await queries.GetByIdAsync(
            request.Id,
            cancellationToken))!;
    }
}
