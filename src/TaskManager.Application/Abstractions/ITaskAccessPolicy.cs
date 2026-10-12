namespace TaskManager.Application.Abstractions;

public sealed record TaskQueryScope(
    int? TeamId,
    int? AssigneeId);

public interface ITaskAccessPolicy
{
    int ResolveTeamIdForCreate(int? requestedTeamId);

    TaskQueryScope ResolveTaskQueryScope();

    void EnsureNotEmployee();

    void EnsureCanAccess(int taskTeamId, int? taskAssigneeId);
}