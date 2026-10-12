namespace TaskManager.Application.Common.Exceptions;

public sealed class NotFoundException(string message) : Exception(message);

public sealed class BadRequestException(string message) : Exception(message);

public sealed class ForbiddenException(string message = "You do not have permission to perform this action.")
    : Exception(message);

public sealed class UnauthorizedException(string message = "Authentication is required.")
    : Exception(message);
