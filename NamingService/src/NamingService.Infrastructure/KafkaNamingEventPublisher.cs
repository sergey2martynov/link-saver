using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using NamingService.Application.Events;
using NamingService.Application.Interfaces;

namespace NamingService.Infrastructure;

// Publishes 'links.named' events to Kafka after Claude generates a title.
// Keyed by LinkId so all events for the same link land on the same partition.
public class KafkaNamingEventPublisher(IProducer<string, string> producer, ILogger<KafkaNamingEventPublisher> logger)
    : INamingEventPublisher
{
    private const string Topic = "links.named";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishLinkNamedAsync(LinkNamedEvent evt, CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            Key = evt.LinkId.ToString(),
            Value = JsonSerializer.Serialize(evt, JsonOptions)
        };

        try
        {
            await producer.ProduceAsync(Topic, message, ct);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex, "Kafka delivery failed for {Topic}: {Error}", Topic, ex.Error.Reason);
        }
    }
}
