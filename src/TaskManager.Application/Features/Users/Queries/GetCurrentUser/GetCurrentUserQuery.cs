
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;
