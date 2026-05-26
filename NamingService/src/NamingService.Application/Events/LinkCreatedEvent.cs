namespace NamingService.Application.Events;

// Mirrors the LinkCreatedEvent published by LinkService to the 'links.created' Kafka topic.
public record LinkCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid LinkId,
    Guid UserId,
    string Url,
    string Title,
    IReadOnlyList<string> Tags);
