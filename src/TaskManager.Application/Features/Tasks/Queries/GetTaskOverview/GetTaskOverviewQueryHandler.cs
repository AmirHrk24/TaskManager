
using MediatR;
using TaskManager.Application.Abstractions;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskOverview;

public sealed class GetTaskOverviewQueryHandler(
    ITaskOverviewService taskOverviewService)
    : IRequestHandler<GetTaskOverviewQuery, IReadOnlyList<TaskOverviewDto>>
{
    public async Task<IReadOnlyList<TaskOverviewDto>> Handle(
        GetTaskOverviewQuery request,
        CancellationToken cancellationToken)
    {
        return await taskOverviewService.GetAllAsync(cancellationToken);
    }
}
