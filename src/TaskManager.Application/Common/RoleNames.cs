namespace TaskManager.Application.Common;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string TeamManager = "TeamManager";
    public const string Employee = "Employee";
    public const string AdminOrManager = Admin + "," + TeamManager;
}
