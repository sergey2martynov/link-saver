using NamingService.Application.Events;

namespace NamingService.Application.Interfaces;

// Port for publishing naming results back to LinkService via Kafka.
public interface INamingEventPublisher
{
    Task PublishLinkNamedAsync(LinkNamedEvent evt, CancellationToken ct = default);
}
