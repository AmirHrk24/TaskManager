
using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<IReadOnlyList<UserDto>>;
