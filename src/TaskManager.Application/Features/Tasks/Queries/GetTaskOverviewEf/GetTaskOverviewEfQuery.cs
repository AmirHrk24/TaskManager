
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskOverviewEf;

public sealed record GetTaskOverviewEfQuery
    : IRequest<IReadOnlyList<TaskOverviewDto>>;
