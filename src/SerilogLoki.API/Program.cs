using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("environment", context.HostingEnvironment.EnvironmentName)
    .Enrich.WithProperty("application", context.HostingEnvironment.ApplicationName)
    .Enrich.WithProperty("machine", Environment.MachineName)
    .WriteTo.Console()
    .WriteTo.GrafanaLoki("http://localhost:3100", labels:
    [
        new LokiLabel { Key = "application", Value = context.HostingEnvironment.ApplicationName },
        new LokiLabel { Key = "environment", Value = context.HostingEnvironment.EnvironmentName },
        new LokiLabel { Key = "machine", Value = Environment.MachineName }
    ]);
});

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("main path executed...");
    return Results.Ok("it works!");
});

app.MapGet("/products", (ILogger<Program> logger) =>
{
    List<string> products = ["computer", "tv", "radio"];
    logger.LogInformation("{Count} products listing...", products.Count);
    return Results.Ok(products);
});

app.Run();
