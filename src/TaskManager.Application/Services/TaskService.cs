using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public sealed class TaskService(
    ITaskQueries taskQueries,
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ITaskAccessPolicy accessPolicy,
    ITaskStatusPolicy statusPolicy) : ITaskService
{
    public async Task<TaskDto> CreateAsync(
        CreateTaskRequest request,
        CancellationToken ct = default)
    {
        var teamId = accessPolicy.ResolveTeamIdForCreate(request.TeamId);

        _ = await uow.GetByIdAsync<Team>([teamId], ct)
            ?? throw new NotFoundException("Team not found.");

        if (request.AssignedToUserId.HasValue)
        {
            await EnsureValidAssigneeAsync(
                request.AssignedToUserId.Value,
                teamId,
                ct);
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            DueDate = request.DueDate,
            TeamId = teamId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = currentUser.UserId
        };

        uow.Add(task);
        await uow.SaveChangesAsync(ct);

        return (await taskQueries.GetByIdAsync(task.Id, ct))!;
    }

    public async Task<TaskDto> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var dto = await taskQueries.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(dto.TeamId, dto.AssignedToUserId);

        return dto;
    }

    public async Task<PagedResult<TaskDto>> GetPagedAsync(
        TaskFilter filter,
        CancellationToken ct = default)
    {
        var scope = accessPolicy.ResolveTaskQueryScope();

        var normalized = filter with
        {
            Page = Math.Max(1, filter.Page),
            PageSize = Math.Clamp(filter.PageSize, 1, 100)
        };

        return await taskQueries.GetPagedAsync(
            normalized,
            scope.TeamId,
            scope.AssigneeId,
            ct);
    }

    public async Task<TaskDto> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken ct = default)
    {
        accessPolicy.EnsureNotEmployee();

        var task = await uow.GetByIdAsync<TaskItem>([id], ct)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(task.TeamId, task.AssignedToUserId);

        if (request.AssignedToUserId.HasValue)
        {
            await EnsureValidAssigneeAsync(
                request.AssignedToUserId.Value,
                task.TeamId,
                ct);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToUserId = request.AssignedToUserId;

        await uow.SaveChangesAsync(ct);

        return (await taskQueries.GetByIdAsync(id, ct))!;
    }

    public async Task<TaskDto> UpdateStatusAsync(
        int id,
        UpdateTaskStatusRequest request,
        CancellationToken ct = default)
    {
        var task = await uow.GetByIdAsync<TaskItem>([id], ct)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(task.TeamId, task.AssignedToUserId);

        if (!statusPolicy.CanChange(task.Status, request.Status))
        {
            throw new BadRequestException(
                $"Cannot change task status from {task.Status} to {request.Status}.");
        }

        task.ChangeStatus(request.Status);

        await uow.SaveChangesAsync(ct);

        return (await taskQueries.GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        accessPolicy.EnsureNotEmployee();

        var task = await uow.GetByIdAsync<TaskItem>([id], ct)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(task.TeamId, task.AssignedToUserId);

        uow.Remove(task);

        await uow.SaveChangesAsync(ct);
    }

    private async Task EnsureValidAssigneeAsync(
        int userId,
        int teamId,
        CancellationToken ct)
    {
        var assignee = await uow.GetByIdAsync<User>([userId], ct)
            ?? throw new BadRequestException("Assignee not found.");

        if (!assignee.IsActive || assignee.TeamId != teamId)
        {
            throw new BadRequestException(
                "The assignee must be an active member of the task's team.");
        }
    }
}