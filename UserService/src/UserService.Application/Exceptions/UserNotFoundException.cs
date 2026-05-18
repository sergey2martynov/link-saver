namespace UserService.Application.Exceptions;

public class UserNotFoundException(Guid id)
    : Exception($"User with ID '{id}' was not found.");
