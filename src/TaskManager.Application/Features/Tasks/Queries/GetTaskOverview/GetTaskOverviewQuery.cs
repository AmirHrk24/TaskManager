
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskOverview;

public sealed record GetTaskOverviewQuery
    : IRequest<IReadOnlyList<TaskOverviewDto>>;
