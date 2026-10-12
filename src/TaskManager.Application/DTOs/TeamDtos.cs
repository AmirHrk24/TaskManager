using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs;

public sealed record CreateTeamRequest
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }
}

public sealed record SetTeamManagerRequest
{
    [Required]
    public int ManagerId { get; init; }
}

public sealed class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int MembersCount { get; set; }
}
