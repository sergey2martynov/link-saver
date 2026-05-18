namespace LinkService.Application.Exceptions;

public class LinkNotFoundException(Guid id)
    : Exception($"Link with ID '{id}' was not found.");
