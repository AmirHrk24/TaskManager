using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Tasks.Commands.CreateTask;
using TaskManager.Application.Features.Tasks.Queries.GetPagedTasks;
using TaskManager.Application.Features.Tasks.Queries.GetTaskById;
using TaskManager.Application.Services;
using TaskManager.Infrastructure.Services;
using TaskManager.Application.Features.Tasks.Commands.UpdateTask;
using TaskManager.Application.Features.Tasks.Commands.DeleteTask;
using TaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController(
    ITaskOverviewService taskOverviewService,
    ITaskOverviewEfService taskOverviewEfService,
    ITaskDetailsEfQuery taskDetailsEfQuery,
    ISender sender) : ControllerBase
{
    /// <summary>
    /// Admin: all tasks.
    /// TeamManager: own team.
    /// Employee: own tasks.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll(
        [FromQuery] TaskFilter filter,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetPagedTasksQuery(filter),
            ct);

        return Ok(result);
    }

    [HttpGet("overview")]
    public async Task<ActionResult<IReadOnlyList<TaskOverviewDto>>> GetOverview(
        CancellationToken ct)
    {
        var result = await taskOverviewService.GetAllAsync(ct);

        return Ok(result);
    }

    [HttpGet("overview/ef")]
    public async Task<ActionResult<IReadOnlyList<TaskOverviewDto>>> GetOverviewEf(
        CancellationToken ct)
    {
        var result = await taskOverviewEfService.GetAllAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:int}/details/ef")]
    public async Task<ActionResult<TaskDetailsDto>> GetDetailsEf(
        int id,
        CancellationToken ct)
    {
        var task = await taskDetailsEfQuery.GetByIdAsync(id, ct);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(
        int id,
        CancellationToken ct)
    {
        var task = await sender.Send(
            new GetTaskByIdQuery(id),
            ct);

        return Ok(task);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var task = await sender.Send(
            new CreateTaskCommand(request),
            ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            task);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<ActionResult<TaskDto>> Update(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var task = await sender.Send(
            new UpdateTaskCommand(id, request),
            ct);
        return Ok(task);
    }

    /// <summary>
    /// All roles can change status, subject to access and status policies.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskDto>> UpdateStatus(
        int id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken ct)
    {
        var task = await sender.Send(
            new UpdateTaskStatusCommand(id, request),
            ct);
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.AdminOrManager)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        await sender.Send(
            new DeleteTaskCommand(id),
            ct);

        return NoContent();
    }
}
