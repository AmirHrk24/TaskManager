
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Commands.UpdateTaskStatus;

public sealed record UpdateTaskStatusCommand(
    int Id,
    UpdateTaskStatusRequest Request) : IRequest<TaskDto>;
