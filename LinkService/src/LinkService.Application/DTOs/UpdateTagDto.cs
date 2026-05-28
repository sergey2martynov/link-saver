namespace LinkService.Application.DTOs;

public record UpdateTagDto(
    string Name,
    string Color,
    DateTime? StartDate = null,
    DateTime? EndDate = null);
