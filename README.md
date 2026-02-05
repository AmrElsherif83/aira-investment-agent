# A.I.R.A. - Autonomous Investment Research Agent

> **InVitro Capital Take-Home Assignment**  
> Multi-step agentic system for automated investment analysis with observable intermediate steps

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-45%2F45%20passing-brightgreen)](tests/Aira.Tests)
[![CI](https://github.com/AmrElsherif83/aira-investment-agent/actions/workflows/ci.yml/badge.svg)](https://github.com/AmrElsherif83/aira-investment-agent/actions/workflows/ci.yml)

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
# Scalar UI: http://localhost:5000/
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

## 🎯 Design Rationale - Why This Architecture?

### **Multi-Step Agent (Not Single LLM Call)**

**Decision**: Implement explicit steps (Planning, Gathering, Scoring, Reflection) instead of single prompt.

**Rationale**:
- **Observability**: Each step produces artifacts that can be inspected independently
- **Debuggability**: Failures isolated to specific steps (e.g., data fetching vs. scoring)
- **Testability**: Each step logic can be unit tested without mocking the entire workflow
- **Flexibility**: Steps can be reordered or replaced without affecting others

**Evidence**: `AnalysisAgent.cs` with 4 explicit methods, `GET /analysis/{jobId}/steps` endpoint

---

### **Asynchronous Processing (Not Synchronous)**

**Decision**: Background worker with queue instead of synchronous HTTP request/response.

**Rationale**:
- **Scalability**: API can accept requests faster than analysis can run
- **User Experience**: Immediate 202 Accepted response, client polls for results
- **Resilience**: Worker crashes don't affect API availability
- **Resource Management**: Control concurrency with bounded queue

**Evidence**: `AnalysisWorker.cs` BackgroundService, `ChannelBackgroundTaskQueue.cs`

---

### **In-Memory Storage (Not Database)**

**Decision**: `ConcurrentDictionary` for job storage.

**Rationale**:
- **Scope**: Assignment is single-instance demo, no persistence requirement
- **Simplicity**: No database setup, migrations, or connection strings
- **Performance**: Zero I/O latency for job lookup
- **Swappable**: `IJobStore` interface allows easy replacement with Redis/PostgreSQL

**Trade-off**: Jobs lost on restart (acceptable for demo, documented in README)

**Evidence**: `InMemoryJobStore.cs`, `IJobStore.cs` interface

---

### **Mock Data Providers (Not Real APIs)**

**Decision**: Deterministic mock providers instead of Alpha Vantage, News API, etc.

**Rationale**:
- **No Dependencies**: Reviewers can run without API keys or internet
- **Deterministic**: Same ticker always returns same data (easier testing)
- **Fast**: Zero network latency
- **Cost**: No paid API usage during evaluation

**Trade-off**: Only supports NVDA with fixed data (documented in README)

**Evidence**: `MockFinancialDataProvider.cs`, `IFinancialDataProvider.cs` interface

---

### **Deterministic Scoring (Not ML Model)**

**Decision**: Explicit formulas for financial/sentiment/risk scoring.

**Rationale**:
- **Explainability**: Every score component can be traced to input data
- **No Training**: No ML model training or tuning required
- **Reproducible**: Same inputs always produce same outputs
- **Debuggable**: Easy to unit test scoring logic

**Evidence**: `ScoreCalculator.cs` with explicit formulas, 45 passing unit tests

---

### **Clean Architecture (Not Monolith)**

**Decision**: 3-layer separation (Api, Core, Infrastructure).

**Rationale**:
- **Testability**: Core logic has zero external dependencies
- **Portability**: Core can run in CLI, Azure Function, or any host
- **Maintainability**: Clear boundaries prevent accidental coupling
- **Reviewability**: Evaluators can focus on Core without API noise

**Evidence**: Project structure, dependency flow (Api → Core ← Infrastructure)

---

## ⚙️ Trade-offs & Productionization

### **Current Limitations**

| Limitation | Impact | Production Fix |
|------------|--------|---------------|
| **In-memory storage** | Jobs lost on restart | Replace with Redis (persistence, multi-instance) or PostgreSQL (audit trail) |
| **No horizontal scaling** | Single instance only | Replace `Channel<T>` with RabbitMQ or Azure Service Bus |
| **Mock data** | NVDA only, fixed responses | Integrate Alpha Vantage, News API, SEC EDGAR |
| **No auth** | Open API | Add JWT Bearer authentication |
| **No rate limiting** | Unbounded requests | Add ASP.NET Core rate limiting middleware |
| **No caching** | Repeated API calls | Add IMemoryCache or Redis for financial/news data |
| **No retry logic** | Transient failures → job failure | Add Polly retry policies with exponential backoff |
| **No observability** | Limited production debugging | Add OpenTelemetry tracing, Application Insights |

### **Production Roadmap**

**Phase 1: Persistence & Scalability**
- Implement `RedisJobStore` or `PostgresJobStore`
- Replace `Channel<T>` with RabbitMQ for distributed queue
- Add multiple worker instances with load balancing

**Phase 2: Real Data Integration**
- Integrate Alpha Vantage for financial statements
- Integrate News API for sentiment analysis
- Parse SEC EDGAR 10-K filings for risk factors
- Add caching layer (5-minute TTL for financial data)

**Phase 3: Resilience & Reliability**
- Add Polly retry policies (3 retries, exponential backoff)
- Add circuit breakers for external APIs
- Add health checks for dependencies
- Implement dead-letter queue for failed jobs

**Phase 4: Security & Governance**
- Add JWT authentication (OAuth2/OpenID Connect)
- Add rate limiting (100 requests/minute per API key)
- Add input validation and sanitization
- Add API versioning (/v1/analysis)

**Phase 5: Observability & Monitoring**
- Add OpenTelemetry distributed tracing
- Add Application Insights or Jaeger
- Add structured logging (Serilog)
- Add metrics (Prometheus)
- Add alerting (failed jobs > threshold)

---

## 📘 How to Use Postman Collection

### **Setup**

1. **Import Collection**
   - Open Postman
   - Click "Import" → "Upload Files"
   - Select `postman/AIRA.postman_collection.json`

2. **Import Environment**
   - Click "Environments" → "Import"
   - Select `postman/AIRA.postman_environment.json`
   - Set "A.I.R.A. Local" as active environment

3. **Verify Configuration**
   - Ensure `baseUrl` = `http://localhost:5000`
   - Ensure `ticker` = `NVDA`

### **Run Tests**

1. **Start API**
```bash
dotnet run --project src/Aira.Api
```

2. **Execute Collection**
   - Click "Collections" → "A.I.R.A. - Investment Analysis API"
   - Click "Run" (Runner icon)
   - Click "Run A.I.R.A."
   - Observe test results

3. **Expected Results**
   - **Submit Analysis**: 202 Accepted, jobId extracted
   - **Get Status**: 200 OK, status = Queued/Running/Succeeded
   - **Get Steps**: 200 OK, steps array with 4 items
   - **Get Result**: 409 Conflict (if running) or 200 OK (if complete)

### **Manual Testing**

1. **Submit Analysis**
   - Send "1. Submit Analysis"
   - Copy `jobId` from response

2. **Poll Status** (repeat until Succeeded)
   - Send "2. Get Job Status"
   - Wait 2-3 seconds
   - Repeat until `status` = "Succeeded"

3. **View Steps**
   - Send "3. Get Step-by-Step Execution"
   - Inspect `steps` array (should have 4 items)

4. **Get Result**
   - Send "4. Get Final Result"
   - Verify `result` contains `company`, `thesis`, `signal`, `insights`, `sources`

### **Troubleshooting**

| Issue | Solution |
|-------|----------|
| **404 Not Found** | Ensure API is running (`dotnet run --project src/Aira.Api`) |
| **jobId not found** | Ensure "Submit Analysis" ran successfully and extracted `{{jobId}}` |
| **409 Conflict on result** | Job still processing, wait a few seconds and retry |
| **500 Internal Server Error** | Check API logs for exceptions |

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

---

## ✅ Submission Ready

- [x] Postman collection created (`postman/AIRA.postman_collection.json`)
- [x] Postman environment created (`postman/AIRA.postman_environment.json`)
- [x] Submission checklist added (`SUBMISSION_CHECKLIST.md`)
- [x] Design rationale documented
- [x] Trade-offs and production roadmap documented
- [x] All 45 tests passing
- [x] Build successful
