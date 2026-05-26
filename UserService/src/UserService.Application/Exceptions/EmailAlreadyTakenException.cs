namespace UserService.Application.Exceptions;

public class EmailAlreadyTakenException(string email)
    : Exception($"Email '{email}' is already registered.");
