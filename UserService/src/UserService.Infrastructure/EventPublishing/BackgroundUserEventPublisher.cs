using Microsoft.Extensions.Logging;
using UserService.Application.Events;
using UserService.Application.Ports;

namespace UserService.Infrastructure.EventPublishing;

// Fire-and-forget decorator over KafkaUserEventPublisher.
// Returns Task.CompletedTask immediately so the HTTP response is never blocked on Kafka I/O.
// CancellationToken.None is used inside the background task because the HTTP request's token
// is cancelled as soon as the response is sent — we don't want to abort in-flight Kafka calls.
public class BackgroundUserEventPublisher(KafkaUserEventPublisher inner, ILogger<BackgroundUserEventPublisher> logger)
    : IUserEventPublisher
{
    public Task PublishUserRegisteredAsync(UserRegisteredEvent evt, CancellationToken ct = default)
    {
        _ = Task.Run(async () =>
        {
            try { await inner.PublishUserRegisteredAsync(evt, CancellationToken.None); }
            catch (Exception ex) { logger.LogError(ex, "Unhandled error in background publish of {Event}", nameof(UserRegisteredEvent)); }
        });
        return Task.CompletedTask;
    }
}
