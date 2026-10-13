
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskDetailsEf;

public sealed class GetTaskDetailsEfQueryHandler(
    ITaskDetailsEfQuery taskDetailsEfQuery)
    : IRequestHandler<GetTaskDetailsEfQuery, TaskDetailsDto?>
{
    public async Task<TaskDetailsDto?> Handle(
        GetTaskDetailsEfQuery request,
        CancellationToken cancellationToken)
    {
        return await taskDetailsEfQuery.GetByIdAsync(
            request.Id,
            cancellationToken);
    }
}
