namespace UserService.Application.Events;

// Domain event emitted when a user registers with email + password.
// Published to the 'users.registered' Kafka topic.
public record UserRegisteredEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid UserId,
    string Email,
    string Name);
