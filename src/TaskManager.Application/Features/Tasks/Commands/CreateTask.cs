using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    CreateTaskRequest Request) : IRequest<TaskDto>;