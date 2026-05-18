using Dapper;
using FluentMigrator.Runner;
using LinkService.Application.Ports;
using LinkService.Infrastructure.Migrations;
using LinkService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LinkService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<ILinkRepository, LinkRepository>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateLinksTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}
