
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskDetailsEf;

public sealed record GetTaskDetailsEfQuery(int Id)
    : IRequest<TaskDetailsDto?>;
