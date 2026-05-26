namespace LinkService.Application.Events;

// Domain event emitted when a link is successfully saved.
// Published to the 'links.created' Kafka topic.
public record LinkCreatedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid LinkId,
    Guid UserId,
    string Url,
    string Title,
    IReadOnlyList<string> Tags);
