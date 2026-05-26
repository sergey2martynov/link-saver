using System.Text.Json;
using Confluent.Kafka;
using LinkService.Application.Events;
using LinkService.Application.Ports;
using Microsoft.Extensions.Logging;

namespace LinkService.Infrastructure.EventPublishing;

// Publishes link domain events to Kafka using Confluent.Kafka.
// Messages are JSON-serialized (camelCase) and keyed by LinkId so all events
// for a given link land on the same partition, preserving order.
public class KafkaLinkEventPublisher(IProducer<string, string> producer, ILogger<KafkaLinkEventPublisher> logger)
    : ILinkEventPublisher
{
    // Kafka topic names — consumers subscribe to these to react to link lifecycle events.
    private const string LinkCreatedTopic = "links.created";
    private const string LinkDeletedTopic = "links.deleted";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishLinkCreatedAsync(LinkCreatedEvent evt, CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            // Key = LinkId ensures all events for the same link go to the same partition.
            Key = evt.LinkId.ToString(),
            Value = JsonSerializer.Serialize(evt, JsonOptions)
        };

        try
        {
            await producer.ProduceAsync(LinkCreatedTopic, message, ct);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex, "Kafka delivery failed for {Event} on topic {Topic}. Error: {Error}",
                nameof(LinkCreatedEvent), LinkCreatedTopic, ex.Error.Reason);
        }
    }

    public async Task PublishLinkDeletedAsync(LinkDeletedEvent evt, CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            Key = evt.LinkId.ToString(),
            Value = JsonSerializer.Serialize(evt, JsonOptions)
        };

        try
        {
            await producer.ProduceAsync(LinkDeletedTopic, message, ct);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex, "Kafka delivery failed for {Event} on topic {Topic}. Error: {Error}",
                nameof(LinkDeletedEvent), LinkDeletedTopic, ex.Error.Reason);
        }
    }
}
