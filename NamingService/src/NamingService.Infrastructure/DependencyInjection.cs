using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NamingService.Application.Interfaces;

namespace NamingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string anthropicApiKey,
        string kafkaBootstrapServers)
    {
        // HttpClient factory — AnthropicClient pulls a client per request via IHttpClientFactory
        services.AddHttpClient();

        services.AddSingleton<IAnthropicClient>(sp =>
            new AnthropicClient(
                sp.GetRequiredService<IHttpClientFactory>(),
                anthropicApiKey,
                sp.GetRequiredService<ILogger<AnthropicClient>>()));

        // Kafka producer: thread-safe singleton; batches and compresses internally
        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(
                    new ProducerConfig { BootstrapServers = kafkaBootstrapServers })
                .Build());

        services.AddSingleton<INamingEventPublisher>(sp =>
            new KafkaNamingEventPublisher(
                sp.GetRequiredService<IProducer<string, string>>(),
                sp.GetRequiredService<ILogger<KafkaNamingEventPublisher>>()));

        // The consumer worker drives the entire naming pipeline as a background task
        services.AddHostedService(sp =>
            new KafkaLinkCreatedConsumer(
                sp.GetRequiredService<IAnthropicClient>(),
                sp.GetRequiredService<INamingEventPublisher>(),
                sp.GetRequiredService<ILogger<KafkaLinkCreatedConsumer>>(),
                kafkaBootstrapServers));

        return services;
    }
}
