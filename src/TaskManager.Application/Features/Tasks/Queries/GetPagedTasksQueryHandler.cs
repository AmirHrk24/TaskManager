
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetPagedTasks;

public sealed class GetPagedTasksQueryHandler(
    ITaskQueries taskQueries,
    ITaskAccessPolicy accessPolicy)
    : IRequestHandler<GetPagedTasksQuery, PagedResult<TaskDto>>
{
    public async Task<PagedResult<TaskDto>> Handle(
        GetPagedTasksQuery request,
        CancellationToken cancellationToken)
    {
        var scope = accessPolicy.ResolveTaskQueryScope();

        return await taskQueries.GetPagedAsync(
            request.Filter,
            scope.TeamId,
            scope.AssigneeId,
            cancellationToken);
    }
}
