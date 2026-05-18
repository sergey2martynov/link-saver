using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using UserService.Application.Ports;
using UserService.Infrastructure.Migrations;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateUsersTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }
}
