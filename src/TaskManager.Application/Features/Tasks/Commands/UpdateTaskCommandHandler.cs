
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler(
    IUnitOfWork uow,
    ITaskQueries taskQueries,
    ITaskAccessPolicy accessPolicy)
    : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(
        UpdateTaskCommand command,
        CancellationToken cancellationToken)
    {
        accessPolicy.EnsureNotEmployee();

        var id = command.Id;
        var request = command.Request;

        var task = await uow.GetByIdAsync<TaskItem>(
            [id],
            cancellationToken)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(
            task.TeamId,
            task.AssignedToUserId);

        if (request.AssignedToUserId.HasValue)
        {
            await EnsureValidAssigneeAsync(
                request.AssignedToUserId.Value,
                task.TeamId,
                cancellationToken);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.AssignedToUserId = request.AssignedToUserId;

        await uow.SaveChangesAsync(cancellationToken);

        return await taskQueries.GetByIdAsync(
            id,
            cancellationToken)
            ?? throw new NotFoundException(
                "Task was updated, but could not be retrieved.");
    }

    private async Task EnsureValidAssigneeAsync(
        int userId,
        int teamId,
        CancellationToken cancellationToken)
    {
        var assignee = await uow.GetByIdAsync<User>(
            [userId],
            cancellationToken)
            ?? throw new BadRequestException("Assignee not found.");

        if (!assignee.IsActive || assignee.TeamId != teamId)
        {
            throw new BadRequestException(
                "The assignee must be an active member of the task's team.");
        }
    }
}
