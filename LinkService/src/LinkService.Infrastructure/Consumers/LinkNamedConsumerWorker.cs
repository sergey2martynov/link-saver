using System.Text.Json;
using Confluent.Kafka;
using LinkService.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LinkService.Infrastructure.Consumers;

// Consumes 'links.named' events published by NamingService after Claude generates a title.
// Applies the AI-generated name to the link so the user sees it on next page load.
public class LinkNamedConsumerWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<LinkNamedConsumerWorker> logger,
    string bootstrapServers) : BackgroundService
{
    private const string Topic = "links.named";
    private const string GroupId = "linkservice-naming";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // EnableAutoCommit=false: commit only after the name is saved, preventing silent data loss on crash
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(Topic);

        logger.LogInformation("LinkNamedConsumerWorker started — consuming {Topic}", Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var evt = JsonSerializer.Deserialize<LinkNamedMessage>(result.Message.Value, JsonOptions);

                if (evt is not null)
                {
                    // LinksService is scoped; create a scope per message
                    using var scope = scopeFactory.CreateScope();
                    var linksService = scope.ServiceProvider.GetRequiredService<LinksService>();
                    await linksService.ApplyNameAsync(evt.LinkId, evt.Name, stoppingToken);
                }

                consumer.Commit(result);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {Topic} message", Topic);
                await Task.Delay(1000, stoppingToken);
            }
        }

        consumer.Close();
    }

    private record LinkNamedMessage(Guid LinkId, string Name);
}
