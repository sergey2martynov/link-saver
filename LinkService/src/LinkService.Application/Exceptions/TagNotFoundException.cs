namespace LinkService.Application.Exceptions;

public class TagNotFoundException(Guid id)
    : Exception($"Tag with ID '{id}' was not found.");
