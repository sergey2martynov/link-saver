namespace LinkService.Application.DTOs;

public record CreateLinkDto(
    string Url,
    string Title,
    string? Description = null,
    List<string>? Tags = null);
