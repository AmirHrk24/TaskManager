using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(ITaskService taskService, ITaskOverviewService taskOverviewService, ITaskOverviewEfService taskOverviewEfService) : ControllerBase
{
    /// <summary>Admin: all tasks. TeamManager: own team. Employee: own tasks.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskFilter filter, CancellationToken ct)
        => Ok(await taskService.GetPagedAsync(filter, ct));
   
    [HttpGet("overview")]
    public async Task<ActionResult<IReadOnlyList<TaskOverviewDto>>> GetOverview(CancellationToken ct)
    => Ok(await taskOverviewService.GetAllAsync(ct));

    [HttpGet("overview/ef")]
    public async Task<ActionResult<IReadOnlyList<TaskOverviewDto>>> GetOverviewEf(
    CancellationToken ct)
    => Ok(await taskOverviewEfService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id, CancellationToken ct)
        => Ok(await taskService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskRequest request, CancellationToken ct)
    {
        var task = await taskService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<ActionResult<TaskDto>> Update(int id, UpdateTaskRequest request, CancellationToken ct)
        => Ok(await taskService.UpdateAsync(id, request, ct));

    /// <summary>All roles (Employee only on tasks assigned to him).</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskDto>> UpdateStatus(int id, UpdateTaskStatusRequest request, CancellationToken ct)
        => Ok(await taskService.UpdateStatusAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await taskService.DeleteAsync(id, ct);
        return NoContent();
    }
}
