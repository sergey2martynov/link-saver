namespace LinkService.Application.DTOs;

public record TagDto(
    Guid Id,
    string Name,
    string Color,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
