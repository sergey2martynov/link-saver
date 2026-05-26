namespace LinkService.Application.Events;

// Domain event emitted when a link is removed.
// Published to the 'links.deleted' Kafka topic.
public record LinkDeletedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid LinkId,
    Guid UserId);
