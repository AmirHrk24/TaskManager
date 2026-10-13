
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(CreateUserRequest Request)
    : IRequest<UserDto>;
