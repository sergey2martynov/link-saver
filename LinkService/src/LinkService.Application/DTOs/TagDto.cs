namespace LinkService.Application.DTOs;

public record TagDto(
    Guid Id,
    string Name,
    string Color,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
