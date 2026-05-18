using FluentMigrator.Runner;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<UsersService>();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Postgres")!);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
