using FluentMigrator.Runner;
using LinkService.Application;
using LinkService.Application.Interfaces;
using LinkService.Infrastructure;
using LinkService.Web.Hubs;
using LinkService.Web.Middleware;
using LinkService.Web.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddScoped<ILinkNotifier, HubLinkNotifier>();
builder.Services.AddScoped<LinksService>();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Postgres")!,
    builder.Configuration["Kafka:BootstrapServers"]!);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<LinksHub>("/hubs/links");

app.Run();
