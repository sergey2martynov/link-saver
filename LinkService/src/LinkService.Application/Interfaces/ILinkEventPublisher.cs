using LinkService.Application.Events;

namespace LinkService.Application.Ports;

// Port for publishing link domain events.
// The Application layer depends on this abstraction; Infrastructure provides the Kafka implementation.
public interface ILinkEventPublisher
{
    Task PublishLinkCreatedAsync(LinkCreatedEvent evt, CancellationToken ct = default);
    Task PublishLinkDeletedAsync(LinkDeletedEvent evt, CancellationToken ct = default);
}
