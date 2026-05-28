namespace LinkService.Application.DTOs;

public record LinkDto(
    Guid Id,
    string Url,
    string Title,
    IReadOnlyCollection<TagDto> Tags,
    IReadOnlyCollection<string> SuggestedTags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
