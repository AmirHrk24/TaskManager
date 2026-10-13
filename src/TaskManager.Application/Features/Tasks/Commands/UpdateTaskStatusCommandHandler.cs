
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;

public sealed class UpdateTaskStatusCommandHandler(
    IUnitOfWork uow,
    ITaskQueries taskQueries,
    ITaskAccessPolicy accessPolicy,
    ITaskStatusPolicy statusPolicy)
    : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
    public async Task<TaskDto> Handle(
        UpdateTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var task = await uow.GetByIdAsync<TaskItem>(
            [command.Id],
            cancellationToken)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(
            task.TeamId,
            task.AssignedToUserId);

        if (!statusPolicy.CanChange(
                task.Status,
                command.Request.Status))
        {
            throw new BadRequestException(
                "This status transition is not allowed.");
        }

        task.ChangeStatus(command.Request.Status);

        await uow.SaveChangesAsync(cancellationToken);

        return await taskQueries.GetByIdAsync(
            task.Id,
            cancellationToken)
            ?? throw new NotFoundException(
                "Task status was updated, but could not be retrieved.");
    }
}
