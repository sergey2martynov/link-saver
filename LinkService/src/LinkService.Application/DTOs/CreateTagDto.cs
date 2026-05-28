namespace LinkService.Application.DTOs;

public record CreateTagDto(
    string Name,
    string Color,
    DateTime? StartDate = null,
    DateTime? EndDate = null);
