using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NamingService.Application.Events;
using NamingService.Application.Interfaces;

namespace NamingService.Infrastructure;

// BackgroundService that drives the full naming pipeline:
//   1. Consumes 'links.created' from LinkService
//   2. Skips links that already have a user-provided title
//   3. Calls Claude Haiku to generate a descriptive title from the URL
//   4. Publishes the result to 'links.named' for LinkService to apply
public class KafkaLinkCreatedConsumer(
    IAnthropicClient anthropicClient,
    INamingEventPublisher publisher,
    ILogger<KafkaLinkCreatedConsumer> logger,
    string bootstrapServers) : BackgroundService
{
    private const string Topic = "links.created";
    // Consumer group scoped to this service so other consumers (e.g. ClassifierService) get their own offset
    private const string GroupId = "namingservice-links";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // EnableAutoCommit=false: we commit only after successfully publishing the name,
        // so a crash before publish causes a retry rather than data loss
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topic);

        logger.LogInformation("KafkaLinkCreatedConsumer started — consuming {Topic}", Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var evt = JsonSerializer.Deserialize<LinkCreatedEvent>(result.Message.Value, JsonOptions);

                if (evt is null)
                {
                    consumer.Commit(result);
                    continue;
                }

                // Skip links where the user already provided a title
                if (!string.IsNullOrWhiteSpace(evt.Title))
                {
                    consumer.Commit(result);
                    continue;
                }

                var name = await anthropicClient.GenerateLinkNameAsync(evt.Url, stoppingToken);
                await publisher.PublishLinkNamedAsync(new LinkNamedEvent(evt.LinkId, name), stoppingToken);

                consumer.Commit(result);
                logger.LogInformation("Named link {LinkId}: {Name}", evt.LinkId, name);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {Topic} message", Topic);
                // Brief pause before retrying to avoid tight error loops
                await Task.Delay(2000, stoppingToken);
            }
        }

        consumer.Close();
    }
}
