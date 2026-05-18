namespace UserService.Application.DTOs;

public record UserDto(Guid Id, string? Email, string Name, DateTime CreatedAt, DateTime? UpdatedAt);
