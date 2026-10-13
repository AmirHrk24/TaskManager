
using MediatR;

namespace TaskManager.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(int Id) : IRequest;
