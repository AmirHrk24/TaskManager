
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler(
    IUnitOfWork uow,
    ITaskAccessPolicy accessPolicy)
    : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(
        DeleteTaskCommand command,
        CancellationToken cancellationToken)
    {
        accessPolicy.EnsureNotEmployee();

        var task = await uow.GetByIdAsync<TaskItem>(
            [command.Id],
            cancellationToken)
            ?? throw new NotFoundException("Task not found.");

        accessPolicy.EnsureCanAccess(
            task.TeamId,
            task.AssignedToUserId);

        uow.Remove(task);

        await uow.SaveChangesAsync(cancellationToken);
    }
}
