
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Common.Exceptions;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler(
    ITaskQueries taskQueries,
    ITaskAccessPolicy accessPolicy)
    : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    public async Task<TaskDto> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = await taskQueries.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }

        accessPolicy.EnsureCanAccess(
            task.TeamId,
            task.AssignedToUserId);

        return task;
    }
}
