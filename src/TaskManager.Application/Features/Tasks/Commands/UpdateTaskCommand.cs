using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    int Id,
    UpdateTaskRequest Request) : IRequest<TaskDto>;