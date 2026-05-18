namespace LinkService.Application.DTOs;

public record UpdateLinkDto(
    string Url,
    string Title,
    string? Description = null,
    List<string>? Tags = null);
