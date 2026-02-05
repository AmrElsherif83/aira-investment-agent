using Aira.Api.Endpoints;
using Aira.Api.Queue;
using Aira.Api.Workers;
using Aira.Core.Agent;
using Aira.Core.Interfaces;
using Aira.Core.Services;
using Aira.Infrastructure.Providers;
using Aira.Infrastructure.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ===== Configure JSON Serialization =====
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Handle Dictionary<string, object> in AgentStepResult.Artifacts
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// ===== Infrastructure Layer Registration =====

// Job Storage - Singleton (in-memory state shared across requests)
builder.Services.AddSingleton<IJobStore, InMemoryJobStore>();

// Background Task Queue - Singleton (shared producer/consumer queue)
builder.Services.AddSingleton<IBackgroundTaskQueue, ChannelBackgroundTaskQueue>();

// Data Providers - Singleton (stateless deterministic mock providers)
builder.Services.AddSingleton<IFinancialDataProvider, MockFinancialDataProvider>();
builder.Services.AddSingleton<INewsProvider, MockNewsProvider>();
builder.Services.AddSingleton<IRiskProvider, MockRiskProvider>();

// ===== Core Layer Registration =====

// Analysis Agent - Singleton (stateless multi-step orchestrator)
builder.Services.AddSingleton<IAnalysisAgent, AnalysisAgent>();

// Analysis Service - Scoped (per-request coordinator)
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

// ===== API Layer Registration =====

// Background Worker - Hosted Service (runs continuously)
builder.Services.AddHostedService<AnalysisWorker>();

// ===== API Documentation =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ===== Configure HTTP Request Pipeline =====

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // OpenAPI spec at /openapi/v1.json
}

app.UseHttpsRedirection();

// ===== Map Endpoints =====
app.MapAnalysisEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTimeOffset.UtcNow,
    service = "AIRA Investment Analysis API"
}))
.WithName("HealthCheck")
.WithTags("Health")
.WithOpenApi();

app.Run();

