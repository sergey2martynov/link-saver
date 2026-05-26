using Confluent.Kafka;
using Dapper;
using FluentMigrator.Runner;
using LinkService.Application.Interfaces;
using LinkService.Application.Ports;
using LinkService.Infrastructure.Consumers;
using LinkService.Infrastructure.EventPublishing;
using LinkService.Infrastructure.Migrations;
using LinkService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LinkService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string kafkaBootstrapServers)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<ILinkRepository, LinkRepository>();
        services.AddScoped<ILinkDomainRepository, LinkDomainRepository>();

        // Kafka: IProducer is thread-safe and designed to be a long-lived singleton.
        // It batches and compresses messages internally for efficiency.
        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(
                new ProducerConfig { BootstrapServers = kafkaBootstrapServers })
            .Build());

        // KafkaLinkEventPublisher does the actual Confluent.Kafka ProduceAsync call.
        // BackgroundLinkEventPublisher wraps it with fire-and-forget semantics (no HTTP blocking).
        services.AddSingleton<KafkaLinkEventPublisher>();
        services.AddSingleton<ILinkEventPublisher>(sp =>
            new BackgroundLinkEventPublisher(
                sp.GetRequiredService<KafkaLinkEventPublisher>(),
                sp.GetRequiredService<ILogger<BackgroundLinkEventPublisher>>()));

        // BackgroundService: applies AI-generated titles received from NamingService via 'links.named'
        services.AddHostedService(sp =>
            new LinkNamedConsumerWorker(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<LinkNamedConsumerWorker>>(),
                kafkaBootstrapServers));

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateLinksTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}
