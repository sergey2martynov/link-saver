namespace LinkService.Application.DTOs;

public record LinkDto(
    Guid Id,
    string Url,
    string Title,
    string? Description,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<string> SuggestedTags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
