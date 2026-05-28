namespace LinkService.Application.DTOs;

public record UpdateLinkDto(
    string Url,
    string Title,
    List<Guid>? TagIds = null);
