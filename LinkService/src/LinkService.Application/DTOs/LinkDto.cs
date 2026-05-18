namespace LinkService.Application.DTOs;

public record LinkDto(
    Guid Id,
    string Url,
    string Title,
    string? Description,
    IReadOnlyCollection<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
