using NamingService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(
    builder.Configuration["Anthropic:ApiKey"]!,
    builder.Configuration["Kafka:BootstrapServers"]!);

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
