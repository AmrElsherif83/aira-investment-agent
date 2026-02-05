# A.I.R.A. - Autonomous Investment Research Agent

> **InVitro Capital Take-Home Assignment**  
> Multi-step agentic system for automated investment analysis with observable intermediate steps

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-45%2F45%20passing-brightgreen)](tests/Aira.Tests)

---

## 🎯 Overview

A.I.R.A. demonstrates a **production-ready agentic architecture** for investment analysis. The system decomposes complex analysis into observable steps, processes jobs asynchronously, and delivers structured, machine-readable outputs with full explainability.

**Key Capabilities:**
- ✅ **Multi-step agent orchestration** - Planning → Gathering → Scoring → Reflection
- ✅ **Asynchronous processing** - Background worker with job queue
- ✅ **Observable steps** - Every intermediate step exposed via API
- ✅ **Deterministic scoring** - Explainable formulas with confidence adjustment
- ✅ **Clean Architecture** - 3-layer separation with testable boundaries
- ✅ **Comprehensive testing** - 45 unit tests, 100% pass rate

---

## 🏗️ Architecture

### Three-Layer Design

```
┌──────────────────────────────────────────────────────┐
│                    Aira.Api                          │
│  • Minimal API Endpoints (POST/GET)                  │
│  • BackgroundService Worker                          │
│  • Swagger/OpenAPI Documentation                     │
└───────────────────┬──────────────────────────────────┘
                    │
                    ▼
┌──────────────────────────────────────────────────────┐
│                   Aira.Core                          │
│  • Domain Models (AnalysisJob, InvestmentReport)    │
│  • Agent Orchestration (4-step workflow)             │
│  • Interfaces (IJobStore, IAnalysisAgent)            │
│  • Scoring Logic (Deterministic formulas)            │
└───────────────────┬──────────────────────────────────┘
                    │
                    ▼
┌──────────────────────────────────────────────────────┐
│                Aira.Infrastructure                   │
│  • InMemoryJobStore (thread-safe)                   │
│  • Mock Data Providers (deterministic)               │
│  • BackgroundTaskQueue (Channel<T>)                  │
└──────────────────────────────────────────────────────┘
```

**Why Three Projects?**

| Project | Purpose | Key Benefit |
|---------|---------|-------------|
| **Aira.Api** | HTTP interface + background worker | Isolates web concerns from business logic |
| **Aira.Core** | Pure domain logic (zero dependencies) | 100% testable, portable to other hosts |
| **Aira.Infrastructure** | External I/O implementations | Swappable (in-memory → Redis/DB) |

---

## 🤖 Agent Workflow

### Four Explicit Steps

```
Planning → Gathering → Scoring → Reflection
   ↓          ↓           ↓          ↓
 Plan     Fetch Data   Calculate  Adjust
Sources   (parallel)     Score   Confidence
```

#### **Step 1: Planning & Decomposition**
- Identifies data sources needed (financials, news, risks)
- **Output**: Analysis plan with source list

#### **Step 2: Data Gathering**
- Fetches data in parallel from 3 providers:
  - Financial metrics (revenue, margins, ROE)
  - News headlines with sentiment
  - Risk factors by severity
- Calculates **data completeness** (0.0-1.0)
- **Output**: Structured snapshots + citations

#### **Step 3: Synthesis & Scoring**
- **Financial Score** (0-100): Base 50 + growth/margin/ROE bonuses
- **Sentiment Score** (0-100): Normalized from -1.0 to +1.0
- **Market Score** (0-100): Base 100 - risk deductions
- **Composite**: Weighted average (40% fin, 30% sent, 30% mkt)
- **Signal**: Bullish (≥65) | Neutral (45-64) | Bearish (<45)
- **Output**: ScoreBreakdown + preliminary signal

#### **Step 4: Reflection & Adjustment**
- Checks for missing data, contradictory signals
- Adjusts confidence based on data quality
- **Safety**: Confidence < 0.40 → force "Neutral" signal
- **Output**: Final report with adjusted confidence

### Observable Execution

All steps are **persisted and retrievable**:

```http
GET /analysis/{jobId}/steps

Response:
[
  {
    "stepName": "Data Gathering",
    "status": "Succeeded",
    "startedAt": "2025-01-17T10:30:01Z",
    "finishedAt": "2025-01-17T10:30:03Z",
    "summary": "Retrieved 100% complete data from all sources",
    "artifacts": {
      "dataCompleteness": 1.0,
      "financialMetricsCount": 5,
      "newsItemsCount": 5,
      "riskItemsCount": 7
    }
  }
]
```

---

## 🚀 Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run Locally

```bash
# Clone repository
git clone https://github.com/AmrElsherif83/aira-investment-agent.git
cd aira-investment-agent

# Run API
dotnet run --project src/Aira.Api

# API available at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### Run Tests

```bash
dotnet test tests/Aira.Tests

# Output:
# Test summary: total: 45, failed: 0, succeeded: 45, duration: 1.6s
```

---

## 📡 API Usage Examples

### 1. Submit Analysis Request

```bash
curl -X POST http://localhost:5000/analysis \
  -H "Content-Type: application/json" \
  -d '{
    "ticker": "NVDA",
    "companyName": "NVIDIA Corporation"
  }'
```

**Response (202 Accepted):**
```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending",
  "submittedAt": "2025-01-17T10:30:00Z"
}
```

### 2. Check Job Status

```bash
curl http://localhost:5000/analysis/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK - Running):**
```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Running",
  "submittedAt": "2025-01-17T10:30:00Z",
  "result": null
}
```

**Response (200 OK - Completed):**
```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "submittedAt": "2025-01-17T10:30:00Z",
  "completedAt": "2025-01-17T10:30:05Z",
  "result": {
    "company": "NVIDIA Corporation",
    "signal": "Bullish",
    "confidence": 0.82,
    "thesis": "NVIDIA demonstrates exceptional financial strength with 126% YoY revenue growth...",
    "scoreBreakdown": {
      "compositeScore": 81.2,
      "financialScore": 91.5,
      "sentimentScore": 70.0,
      "marketScore": 82.0
    }
  }
}
```

### 3. Get Step-by-Step Execution

```bash
curl http://localhost:5000/analysis/550e8400-e29b-41d4-a716-446655440000/steps
```

### 4. Get Result Only

```bash
curl http://localhost:5000/analysis/550e8400-e29b-41d4-a716-446655440000/result
```

---

## 🧪 Testing Strategy

### Unit Tests (45 tests, 100% pass rate)

**SentimentAnalyzerTests (28 tests)**
- ✅ Positive keywords increase score, negative decrease
- ✅ Recency weighting (recent news weighted 5x more than 30-day-old)
- ✅ Mixed sentiment weighted average
- ✅ Edge cases: empty lists, null inputs

**SignalGeneratorTests (17 tests)**
- ✅ Bullish boundary: score ≥ 65 → "Bullish"
- ✅ Neutral boundary: score 45-64 → "Neutral"
- ✅ Bearish boundary: score < 45 → "Bearish"
- ✅ Confidence override: confidence < 0.40 → force "Neutral"

```bash
dotnet test tests/Aira.Tests

# Output:
# Test summary: total: 45, failed: 0, succeeded: 45, duration: 1.6s
# Build succeeded in 2.9s
```

---

## ⚖️ Trade-offs & Limitations

### Current Implementation (MVP Scope)

| Component | Implementation | Rationale |
|-----------|---------------|-----------|
| **Job Store** | In-memory `ConcurrentDictionary` | Single-instance demo; no persistence requirement |
| **Data Providers** | Mock with deterministic NVDA data | No paid APIs; predictable testing |
| **Queue** | In-process `Channel<T>` | Low latency; sufficient for single node |
| **Scoring** | Deterministic formulas | Explainable; no ML training |
| **Auth** | None (open API) | Demo scope |

### Known Limitations

1. **No Persistence** - Jobs lost on restart (acceptable for evaluation)
2. **No Horizontal Scaling** - Single instance only
3. **Mock Data** - NVDA-only with fixed responses
4. **No Rate Limiting** - Unbounded request acceptance
5. **No Authentication** - Open API (add JWT Bearer for production)

---

## 🚢 Production Roadmap

### Phase 1: Persistence Layer

```csharp
// Replace InMemoryJobStore with Redis:
public class RedisJobStore : IJobStore
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task<AnalysisJob?> GetJobAsync(Guid jobId)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"job:{jobId}");
        return json.HasValue 
            ? JsonSerializer.Deserialize<AnalysisJob>(json!) 
            : null;
    }
}
```

**Benefits:** Persistence across restarts, multi-instance support, job history

### Phase 2: Real Data Providers

```csharp
public class AlphaVantageFinancialProvider : IFinancialDataProvider
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    
    public async Task<FinancialSnapshot> GetFinancialSnapshotAsync(string ticker)
    {
        // Cache with 5-minute TTL
        if (_cache.TryGetValue($"fin:{ticker}", out FinancialSnapshot? cached))
            return cached!;
        
        // Call Alpha Vantage API
        var data = await _http.GetFromJsonAsync<AlphaVantageResponse>(
            $"https://alphavantage.co/query?function=INCOME_STATEMENT&symbol={ticker}");
        
        var snapshot = MapToSnapshot(data);
        _cache.Set($"fin:{ticker}", snapshot, TimeSpan.FromMinutes(5));
        return snapshot;
    }
}
```

**Integrate:**
- [Alpha Vantage](https://www.alphavantage.co/) - Financial statements
- [News API](https://newsapi.org/) - Sentiment analysis
- [SEC EDGAR](https://www.sec.gov/edgar) - Risk factors from 10-K filings

### Phase 3: Resilience & Observability

```csharp
// Polly retry policies:
services.AddHttpClient<IFinancialDataProvider, AlphaVantageProvider>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
    ));

// OpenTelemetry distributed tracing:
services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter());
```

### Phase 4: Distributed Queue

```csharp
// Replace Channel with RabbitMQ/Azure Service Bus:
public class RabbitMqTaskQueue : IBackgroundTaskQueue
{
    private readonly IConnection _connection;
    
    public async Task QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ValueTask> workItem)
    {
        var message = JsonSerializer.SerializeToUtf8Bytes(
            new { JobId = Guid.NewGuid() });
        _channel.BasicPublish(
            exchange: "",
            routingKey: "analysis-jobs",
            body: message);
    }
}
```

**Benefits:** Multiple workers, load balancing, dead-letter queues

### Phase 5: Security & Governance

```csharp
// JWT authentication:
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

// Rate limiting:
services.AddRateLimiter(options => 
    options.AddFixedWindowLimiter("api", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    })
);
```

---

## 📊 Sample Output

```json
{
  "company": "NVIDIA Corporation",
  "thesis": "NVIDIA demonstrates exceptional financial strength with 126% year-over-year revenue growth and industry-leading 48.9% profit margins, positioning it as a dominant force in AI acceleration and data center markets. Strong positive market sentiment and manageable risk profile support a bullish outlook.",
  "signal": "Bullish",
  "confidence": 0.82,
  "insights": [
    {
      "type": "financial",
      "detail": "Exceptional financial performance with 126% YoY revenue growth and 48.9% profit margin",
      "impact": 0.8
    },
    {
      "type": "sentiment",
      "detail": "Strong positive sentiment in recent news suggests favorable market perception",
      "impact": 0.6
    },
    {
      "type": "risk",
      "detail": "Elevated risk count (7 factors) suggests complex risk profile",
      "impact": -0.4
    }
  ],
  "sources": [
    "mock://financials/NVDA",
    "mock://news/NVDA",
    "mock://risks/NVDA"
  ],
  "scoreBreakdown": {
    "financialScore": 91.5,
    "sentimentScore": 70.0,
    "marketScore": 82.0,
    "compositeScore": 81.2,
    "financialWeight": 0.40,
    "sentimentWeight": 0.30,
    "marketWeight": 0.30
  },
  "generatedAt": "2025-02-17T10:30:05Z"
}
```

---

## 🎓 Evaluation Criteria Alignment

| Criterion | Implementation | Evidence |
|-----------|---------------|----------|
| **Multi-step agentic behavior** | 4 explicit steps with planning/decomposition | `AnalysisAgent.cs` lines 45-200 |
| **Asynchronous processing** | BackgroundService + Channel queue | `AnalysisWorker.cs`, `BackgroundTaskQueue.cs` |
| **Observable steps** | `/steps` endpoint with artifacts | `AnalysisEndpoints.cs` line 80 |
| **Structured output** | JSON schema with thesis, signal, confidence | `InvestmentReport.cs` |
| **Clean architecture** | 3-layer separation with interface boundaries | Architecture diagram above |
| **Testability** | 45 unit tests, 100% pass rate | `tests/Aira.Tests/` |
| **Deterministic scoring** | Explicit formulas, no randomness | `ScoreCalculator.cs` lines 20-150 |
| **Source citations** | Every data point cites mock:// URIs | `sources` array in output |
| **Confidence adjustment** | Reflection step adjusts based on quality | `AnalysisAgent.cs` lines 180-200 |
| **Production patterns** | DI, logging, config, health checks | `Program.cs` |

---

## 📚 Documentation

- **[Architecture Deep Dive](docs/ARCHITECTURE.md)** - Detailed system design
- **[Requirements Specification](docs/REQUIREMENTS.md)** - Functional and technical specs
- **[Implementation Checklist](docs/IMPLEMENTATION_CHECKLIST.md)** - Development progress
- **[API Testing Guide](docs/API_TESTING_GUIDE.md)** - curl examples and workflows
- **[Configuration Guide](docs/CONFIGURATION_GUIDE.md)** - Environment variables and settings
- **[DI Registration Guide](docs/DI_REGISTRATION_GUIDE.md)** - Dependency injection setup

---

## 📝 License

MIT License - see [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Amr Elsherif**  
InVitro Capital Take-Home Assignment  
February 2025

---

**Built with:** .NET 10 | Minimal APIs | Clean Architecture | Channel-based queues
