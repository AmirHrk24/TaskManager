using TaskManager.Application.Common.Exceptions;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Abstractions;

public sealed class TaskAccessPolicy(
    ICurrentUser currentUser) : ITaskAccessPolicy
{
    public int ResolveTeamIdForCreate(int? requestedTeamId)
    {
        switch (currentUser.Role)
        {
            case UserRole.Admin:
                return requestedTeamId
                    ?? throw new BadRequestException("TeamId is required.");

            case UserRole.TeamManager:
                var ownTeamId = currentUser.TeamId
                    ?? throw new ForbiddenException(
                        "You are not assigned to any team.");

                if (requestedTeamId.HasValue &&
                    requestedTeamId.Value != ownTeamId)
                {
                    throw new ForbiddenException(
                        "You can only create tasks for your own team.");
                }

                return ownTeamId;

            default:
                throw new ForbiddenException(
                    "Employees cannot create tasks.");
        }
    }

    public TaskQueryScope ResolveTaskQueryScope()
    {
        return currentUser.Role switch
        {
            UserRole.Admin => new TaskQueryScope(null, null),

            UserRole.TeamManager => new TaskQueryScope(
                currentUser.TeamId
                    ?? throw new ForbiddenException(
                        "You are not assigned to any team."),
                null),

            UserRole.Employee => new TaskQueryScope(
                null,
                currentUser.UserId),

            _ => throw new ForbiddenException(
                "You do not have permission to view tasks.")
        };
    }

    public void EnsureNotEmployee()
    {
        if (currentUser.Role == UserRole.Employee)
        {
            throw new ForbiddenException(
                "Employees can only change the status of their own tasks.");
        }
    }

    public void EnsureCanAccess(
        int taskTeamId,
        int? taskAssigneeId)
    {
        var allowed = currentUser.Role switch
        {
            UserRole.Admin => true,
            UserRole.TeamManager =>
                currentUser.TeamId == taskTeamId,
            UserRole.Employee =>
                taskAssigneeId == currentUser.UserId,
            _ => false
        };

        if (!allowed)
        {
            throw new ForbiddenException(
                "You do not have access to this task.");
        }
    }
}