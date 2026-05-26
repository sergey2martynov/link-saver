using LinkService.Application.Events;
using LinkService.Application.Ports;
using Microsoft.Extensions.Logging;

namespace LinkService.Infrastructure.EventPublishing;

// Fire-and-forget decorator over KafkaLinkEventPublisher.
// Returns Task.CompletedTask immediately so the HTTP response is never blocked on Kafka I/O.
// The actual Kafka publish runs on a background thread pool thread.
// CancellationToken.None is used inside the background task because the HTTP request's token
// is cancelled as soon as the response is sent — we don't want to abort in-flight Kafka calls.
public class BackgroundLinkEventPublisher(KafkaLinkEventPublisher inner, ILogger<BackgroundLinkEventPublisher> logger)
    : ILinkEventPublisher
{
    public Task PublishLinkCreatedAsync(LinkCreatedEvent evt, CancellationToken ct = default)
    {
        _ = Task.Run(async () =>
        {
            try { await inner.PublishLinkCreatedAsync(evt, CancellationToken.None); }
            catch (Exception ex) { logger.LogError(ex, "Unhandled error in background publish of {Event}", nameof(LinkCreatedEvent)); }
        });
        return Task.CompletedTask;
    }

    public Task PublishLinkDeletedAsync(LinkDeletedEvent evt, CancellationToken ct = default)
    {
        _ = Task.Run(async () =>
        {
            try { await inner.PublishLinkDeletedAsync(evt, CancellationToken.None); }
            catch (Exception ex) { logger.LogError(ex, "Unhandled error in background publish of {Event}", nameof(LinkDeletedEvent)); }
        });
        return Task.CompletedTask;
    }
}
