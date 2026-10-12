using TaskManager.Domain.Enums;

namespace TaskManager.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    UserRole Role { get; }
    int? TeamId { get; }
}
