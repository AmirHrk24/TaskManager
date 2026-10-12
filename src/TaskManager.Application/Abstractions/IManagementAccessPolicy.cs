namespace TaskManager.Application.Abstractions;

public interface IManagementAccessPolicy
{
    void EnsureAdmin();

    int? ResolveTeamScopeForList();
}