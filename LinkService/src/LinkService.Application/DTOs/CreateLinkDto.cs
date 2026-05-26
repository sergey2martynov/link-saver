namespace LinkService.Application.DTOs;

public record CreateLinkDto(
    string Url,
    string? Title = null,
    string? Description = null,
    List<string>? Tags = null);
