using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, serviceProvider, loggerConfiguration) =>
{
    //TODO : get configuration
    string lokiUrl = "http://localhost:3100";
    string logLevelAsString = "Information";

    var level = Enum.TryParse<LogEventLevel>(logLevelAsString, true, out var logLevel)
        ? logLevel
        : LogEventLevel.Information;


    loggerConfiguration
        .Enrich.WithClientIp()
        .Enrich.WithSpan()
        .Enrich.WithCorrelationId()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("application", context.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("machine", Environment.MachineName)
        .WriteTo.Console(new JsonFormatter(renderMessage: true));

    loggerConfiguration
        .MinimumLevel.Is(level)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning);

    if (!context.HostingEnvironment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.GrafanaLoki(lokiUrl, labels:
        [
            new LokiLabel { Key = "application", Value = context.HostingEnvironment.ApplicationName },
            new LokiLabel { Key = "environment", Value = context.HostingEnvironment.EnvironmentName },
            new LokiLabel { Key = "machine", Value = Environment.MachineName }
        ]);
    }
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
