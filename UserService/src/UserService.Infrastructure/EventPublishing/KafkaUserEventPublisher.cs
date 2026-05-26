using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using UserService.Application.Events;
using UserService.Application.Ports;

namespace UserService.Infrastructure.EventPublishing;

// Publishes user domain events to Kafka using Confluent.Kafka.
// Messages are JSON-serialized (camelCase) and keyed by UserId.
public class KafkaUserEventPublisher(IProducer<string, string> producer, ILogger<KafkaUserEventPublisher> logger)
    : IUserEventPublisher
{
    // Kafka topic name — consumers subscribe to this to react when a new user registers.
    private const string UserRegisteredTopic = "users.registered";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishUserRegisteredAsync(UserRegisteredEvent evt, CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            // Key = UserId ensures all events for the same user land on the same partition.
            Key = evt.UserId.ToString(),
            Value = JsonSerializer.Serialize(evt, JsonOptions)
        };

        try
        {
            await producer.ProduceAsync(UserRegisteredTopic, message, ct);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex, "Kafka delivery failed for {Event} on topic {Topic}. Error: {Error}",
                nameof(UserRegisteredEvent), UserRegisteredTopic, ex.Error.Reason);
        }
    }
}
