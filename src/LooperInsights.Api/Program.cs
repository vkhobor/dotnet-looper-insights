using LooperInsights.Api.Metrics;
using LooperInsights.Api.Workers;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LooperMetrics>();
builder.Services.AddHostedService<LooperBackgroundService>();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));

app.UseMetricServer();
app.UseHttpMetrics();

app.Run();
