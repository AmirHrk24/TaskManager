
using MediatR;
using TaskManager.Application.Common.Models;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetPagedTasks;

public sealed record GetPagedTasksQuery(TaskFilter Filter)
    : IRequest<PagedResult<TaskDto>>;
