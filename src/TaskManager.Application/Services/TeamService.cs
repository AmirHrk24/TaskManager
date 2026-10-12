using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public sealed class TeamService(
    ITeamQueries queries,
    IUnitOfWork uow,
    IManagementAccessPolicy accessPolicy) : ITeamService
{
    public async Task<TeamDto> CreateAsync(
        CreateTeamRequest request,
        CancellationToken ct = default)
    {
        accessPolicy.EnsureAdmin();

        var name = request.Name.Trim();

        if (await uow.AnyAsync<Team>(
                team => team.Name == name, ct))
        {
            throw new BadRequestException(
                "A team with this name already exists.");
        }

        var team = new Team
        {
            Name = name,
            Description = request.Description?.Trim()
        };

        uow.Add(team);
        await uow.SaveChangesAsync(ct);

        return (await queries.GetByIdAsync(team.Id, ct))!;
    }

    public async Task<IReadOnlyList<TeamDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var scopeTeamId = accessPolicy.ResolveTeamScopeForList();

        return await queries.GetAllAsync(scopeTeamId, ct);
    }

    public async Task<TeamDto> SetManagerAsync(
        int teamId,
        SetTeamManagerRequest request,
        CancellationToken ct = default)
    {
        accessPolicy.EnsureAdmin();

        var team = await uow.GetByIdAsync<Team>([teamId], ct)
            ?? throw new NotFoundException("Team not found.");

        var user = await uow.GetByIdAsync<User>(
            [request.ManagerId], ct)
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

        if (user.TeamId.HasValue && user.TeamId.Value != teamId)
        {
            throw new BadRequestException(
                "The selected user already belongs to another team.");
        }

        user.TeamId = teamId;
        team.ManagerId = user.Id;

        await uow.SaveChangesAsync(ct);

        return (await queries.GetByIdAsync(teamId, ct))!;
    }
}