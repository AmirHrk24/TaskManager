using TaskManager.Application.Common.Exceptions;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Abstractions;

public sealed class ManagementAccessPolicy(
    ICurrentUser currentUser) : IManagementAccessPolicy
{
    public void EnsureAdmin()
    {
        if (currentUser.Role != UserRole.Admin)
        {
            throw new ForbiddenException(
                "Only admins can perform this operation.");
        }
    }

    public int? ResolveTeamScopeForList()
    {
        return currentUser.Role switch
        {
            UserRole.Admin => null,

            UserRole.TeamManager => currentUser.TeamId
                ?? throw new ForbiddenException(
                    "You are not assigned to any team."),

            _ => throw new ForbiddenException()
        };
    }
}