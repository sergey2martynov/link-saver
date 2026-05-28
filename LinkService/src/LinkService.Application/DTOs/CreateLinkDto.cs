namespace LinkService.Application.DTOs;

public record CreateLinkDto(
    string Url,
    string? Title = null,
    List<Guid>? TagIds = null);
