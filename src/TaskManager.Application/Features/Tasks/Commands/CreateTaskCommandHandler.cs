using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler(
    ITaskQueries taskQueries,
    IUnitOfWork uow,
    ICurrentUser currentUser,
    ITaskAccessPolicy accessPolicy)
    : IRequestHandler<CreateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(
        CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var teamId = accessPolicy.ResolveTeamIdForCreate(
            request.TeamId);

        _ = await uow.GetByIdAsync<Team>(
            [teamId],
            cancellationToken)
            ?? throw new NotFoundException("Team not found.");

        if (request.AssignedToUserId.HasValue)
        {
            await EnsureValidAssigneeAsync(
                request.AssignedToUserId.Value,
                teamId,
                cancellationToken);
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

        await uow.SaveChangesAsync(cancellationToken);

        return await taskQueries.GetByIdAsync(
            task.Id,
            cancellationToken)
            ?? throw new NotFoundException(
                "Task was created, but could not be retrieved.");
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