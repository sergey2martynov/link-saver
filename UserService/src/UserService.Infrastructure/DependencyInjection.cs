using Confluent.Kafka;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using UserService.Application.Ports;
using UserService.Infrastructure.EventPublishing;
using UserService.Infrastructure.Migrations;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Security;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string kafkaBootstrapServers)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Kafka: IProducer is thread-safe and designed to be a long-lived singleton.
        // It batches and compresses messages internally for efficiency.
        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(
                new ProducerConfig { BootstrapServers = kafkaBootstrapServers })
            .Build());

        // KafkaUserEventPublisher does the actual Confluent.Kafka ProduceAsync call.
        // BackgroundUserEventPublisher wraps it with fire-and-forget semantics (no HTTP blocking).
        services.AddSingleton<KafkaUserEventPublisher>();
        services.AddSingleton<IUserEventPublisher>(sp =>
            new BackgroundUserEventPublisher(
                sp.GetRequiredService<KafkaUserEventPublisher>(),
                sp.GetRequiredService<ILogger<BackgroundUserEventPublisher>>()));

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateUsersTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}
