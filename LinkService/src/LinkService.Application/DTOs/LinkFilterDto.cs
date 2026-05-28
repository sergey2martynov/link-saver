namespace LinkService.Application.DTOs;

public record LinkFilterDto(
    Guid[]? TagIds = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null);
