# API Testing Guide

## Starting the Application

```bash
cd src/Aira.Api
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Aira.Api.Workers.AnalysisWorker[0]
      AnalysisWorker is starting
```

---

## API Documentation

**OpenAPI Specification:**
- URL: `https://localhost:7001/openapi/v1.json`
- Format: OpenAPI 3.0 JSON

You can view the spec in:
- Swagger UI: Use `https://editor.swagger.io/` and paste the spec
- Postman: Import the OpenAPI spec
- Any OpenAPI-compatible tool

---

## Endpoint Testing

### 1. Health Check

```bash
GET https://localhost:7001/health
```

**Expected Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T10:00:00Z",
  "service": "AIRA Investment Analysis API"
}
```

---

### 2. Submit Analysis Request

```bash
POST https://localhost:7001/analysis
Content-Type: application/json

{
  "ticker": "NVDA"
}
```

**Expected Response (202 Accepted):**
```json
{
  "jobId": "abc-123-def-456",
  "ticker": "NVDA",
  "status": "Queued",
  "submittedAt": "2024-01-01T10:00:00Z",
  "statusUrl": "https://localhost:7001/analysis/abc-123-def-456",
  "stepsUrl": "https://localhost:7001/analysis/abc-123-def-456/steps",
  "resultUrl": "https://localhost:7001/analysis/abc-123-def-456/result"
}
```

**Invalid Ticker (400 Bad Request):**
```bash
POST https://localhost:7001/analysis
Content-Type: application/json

{
  "ticker": ""
}
```

Response:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ticker": ["Ticker cannot be empty or whitespace."]
  }
}
```

---

### 3. Get Job Status

```bash
GET https://localhost:7001/analysis/{jobId}
```

**Response while Queued (200 OK):**
```json
{
  "jobId": "abc-123-def-456",
  "ticker": "NVDA",
  "status": "Queued",
  "createdAt": "2024-01-01T10:00:00Z",
  "startedAt": null,
  "finishedAt": null,
  "error": null,
  "stepCount": 0,
  "hasResult": false
}
```

**Response while Running (200 OK):**
```json
{
  "jobId": "abc-123-def-456",
  "ticker": "NVDA",
  "status": "Running",
  "createdAt": "2024-01-01T10:00:00Z",
  "startedAt": "2024-01-01T10:00:01Z",
  "finishedAt": null,
  "error": null,
  "stepCount": 2,
  "hasResult": false
}
```

**Response when Succeeded (200 OK):**
```json
{
  "jobId": "abc-123-def-456",
  "ticker": "NVDA",
  "status": "Succeeded",
  "createdAt": "2024-01-01T10:00:00Z",
  "startedAt": "2024-01-01T10:00:01Z",
  "finishedAt": "2024-01-01T10:00:02Z",
  "error": null,
  "stepCount": 4,
  "hasResult": true
}
```

**Job Not Found (404 Not Found):**
```json
{
  "error": "Job 00000000-0000-0000-0000-000000000000 not found."
}
```

---

### 4. Get Analysis Steps

```bash
GET https://localhost:7001/analysis/{jobId}/steps
```

**Expected Response (200 OK):**
```json
{
  "jobId": "abc-123-def-456",
  "ticker": "NVDA",
  "status": "Succeeded",
  "steps": [
    {
      "stepName": "Planning",
      "status": "Succeeded",
      "startedAt": "2024-01-01T10:00:01.000Z",
      "finishedAt": "2024-01-01T10:00:01.020Z",
      "summary": "Created structured analysis plan for NVDA. Identified 3 data sources and 6 key metrics to evaluate.",
      "artifacts": {
        "planning_output": {
          "ticker": "NVDA",
          "plannedSteps": [
            "Data Gathering - Collect financial, news, and risk data",
            "Scoring - Calculate component and composite scores",
            "Synthesis - Generate thesis and insights",
            "Reflection - Validate data quality and adjust confidence"
          ],
          "requiredTools": [
            "IFinancialDataProvider - Financial statements and metrics",
            "INewsProvider - Recent news and sentiment",
            "IRiskProvider - Risk factors and severity assessment"
          ],
          "keyMetrics": [
            "Revenue growth YoY",
            "Profit margins",
            "Return on Equity",
            "News sentiment score",
            "Risk severity distribution",
            "Data completeness"
          ],
          "focusAreas": [
            "Financial performance and growth trajectory",
            "Market sentiment and public perception",
            "Risk factors and competitive positioning",
            "Valuation considerations"
          ]
        },
        "ticker": "NVDA"
      }
    },
    {
      "stepName": "Data Gathering",
      "status": "Succeeded",
      "startedAt": "2024-01-01T10:00:01.020Z",
      "finishedAt": "2024-01-01T10:00:01.120Z",
      "summary": "Gathered data from 3 sources. Financial: ?, News: 5 items, Risks: 7 factors. Data completeness: 100%. 0 warning(s) recorded.",
      "artifacts": {
        "gathering_output": { ... },
        "financial_snapshot": { ... },
        "news_count": 5,
        "risk_count": 7,
        "data_warnings": [],
        "sources": [
          "mock://financials/NVDA",
          "mock://news/NVDA",
          "mock://risks/NVDA"
        ]
      }
    },
    {
      "stepName": "Scoring and Synthesis",
      "status": "Succeeded",
      "startedAt": "2024-01-01T10:00:01.120Z",
      "finishedAt": "2024-01-01T10:00:01.180Z",
      "summary": "Calculated composite score: 77.8/100. Preliminary signal: Bullish (Confidence: 78%). Generated 4 insights and investment thesis.",
      "artifacts": {
        "scoring_output": { ... },
        "score_breakdown": { ... },
        "composite_score": 77.8,
        "preliminary_signal": "Bullish",
        "preliminary_confidence": 0.78
      }
    },
    {
      "stepName": "Reflection and Finalization",
      "status": "Succeeded",
      "startedAt": "2024-01-01T10:00:01.180Z",
      "finishedAt": "2024-01-01T10:00:01.240Z",
      "summary": "Reflection complete. Final signal: Bullish (Confidence: 78%). Data completeness: 100%. Limitations: 0. No adjustments needed.",
      "artifacts": {
        "final_report": { ... },
        "final_signal": "Bullish",
        "final_confidence": 0.78,
        "adjustments_applied": 0,
        "limitations_count": 0,
        "confidence_adjustment": 0
      }
    }
  ]
}
```

---

### 5. Get Analysis Result

```bash
GET https://localhost:7001/analysis/{jobId}/result
```

**When Still Running (409 Conflict):**
```json
{
  "jobId": "abc-123-def-456",
  "status": "Running",
  "message": "Analysis is still in progress. Current status: Running",
  "retryAfterSeconds": 5
}
```

**When Failed (500 Internal Server Error):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Analysis Failed",
  "status": 500,
  "detail": "NullReferenceException: Object reference not set to an instance of an object.",
  "instance": "/analysis/abc-123-def-456/result",
  "jobId": "abc-123-def-456",
  "failedAt": "2024-01-01T10:00:02Z"
}
```

**When Succeeded (200 OK):**
```json
{
  "jobId": "abc-123-def-456",
  "ticker": "NVDA",
  "status": "Succeeded",
  "completedAt": "2024-01-01T10:00:02.240Z",
  "result": {
    "company": "NVIDIA Corporation",
    "thesis": "Investment Analysis for NVIDIA Corporation (NVDA):\n\nSignal: Bullish (Confidence: 78%)\nComposite Score: 77.8/100\n\nScore Breakdown:\n- Financial Health: 100.0/100 (Weight: 40%)\n- Market Sentiment: 70.0/100 (Weight: 30%)\n- Risk Assessment: 44.0/100 (Weight: 30%)\n\nInvestment Rationale:\nThe analysis indicates a favorable investment opportunity for NVIDIA Corporation. Strong financial fundamentals support growth prospects. Positive market sentiment and news flow reinforce the bullish outlook. Risk profile is manageable relative to potential upside.",
    "signal": "Bullish",
    "insights": [
      {
        "type": "financial",
        "detail": "Exceptional financial performance with strong revenue growth and profitability metrics.",
        "impact": 0.8
      },
      {
        "type": "sentiment",
        "detail": "Strong positive sentiment in recent news suggests favorable market perception.",
        "impact": 0.6
      },
      {
        "type": "risk",
        "detail": "Multiple high-severity risks identified (3 critical factors) require careful monitoring.",
        "impact": -0.7
      },
      {
        "type": "valuation",
        "detail": "High composite score suggests strong investment case, but consider current valuation multiples.",
        "impact": 0.5
      }
    ],
    "sources": [
      {
        "type": "financial",
        "ref": "mock://financials/NVDA",
        "notes": null
      },
      {
        "type": "news",
        "ref": "mock://news/NVDA",
        "notes": null
      },
      {
        "type": "risk",
        "ref": "mock://risks/NVDA",
        "notes": null
      }
    ],
    "confidence": 0.78,
    "limitations": null,
    "scoreBreakdown": {
      "financialScore": 100.0,
      "sentimentScore": 70.0,
      "marketScore": 44.0,
      "compositeScore": 77.8,
      "financialWeight": 0.4,
      "sentimentWeight": 0.3,
      "marketWeight": 0.3
    },
    "generatedAt": "2024-01-01T10:00:01.240Z"
  }
}
```

---

## Complete Test Workflow

### Using curl:

```bash
# 1. Submit analysis
RESPONSE=$(curl -s -X POST https://localhost:7001/analysis \
  -H "Content-Type: application/json" \
  -d '{"ticker":"NVDA"}')

echo $RESPONSE | jq '.'

# Extract jobId
JOB_ID=$(echo $RESPONSE | jq -r '.jobId')
echo "Job ID: $JOB_ID"

# 2. Wait a moment
sleep 2

# 3. Check status
curl -s "https://localhost:7001/analysis/$JOB_ID" | jq '.'

# 4. Get steps
curl -s "https://localhost:7001/analysis/$JOB_ID/steps" | jq '.steps[] | {stepName, status, summary}'

# 5. Get result
curl -s "https://localhost:7001/analysis/$JOB_ID/result" | jq '.result | {company, signal, confidence}'
```

### Using PowerShell:

```powershell
# 1. Submit analysis
$response = Invoke-RestMethod -Method Post `
  -Uri "https://localhost:7001/analysis" `
  -ContentType "application/json" `
  -Body '{"ticker":"NVDA"}'

$response | ConvertTo-Json -Depth 10
$jobId = $response.jobId

# 2. Wait a moment
Start-Sleep -Seconds 2

# 3. Check status
Invoke-RestMethod "https://localhost:7001/analysis/$jobId" | ConvertTo-Json

# 4. Get steps
Invoke-RestMethod "https://localhost:7001/analysis/$jobId/steps" | 
  Select-Object -ExpandProperty steps | 
  Select-Object stepName, status, summary

# 5. Get result
$result = Invoke-RestMethod "https://localhost:7001/analysis/$jobId/result"
$result.result | Select-Object company, signal, confidence | ConvertTo-Json
```

---

## Expected Timing

For NVDA analysis:
- **Submit:** ~10ms (immediate)
- **Queue ? Running:** ~1-2 seconds (worker polling)
- **Analysis Execution:** ~200-500ms
  - Planning: ~20ms
  - Gathering: ~100ms (3 provider calls)
  - Scoring: ~60ms
  - Reflection: ~60ms
- **Total Duration:** ~2-3 seconds end-to-end

---

## Troubleshooting

### Worker not processing jobs

**Check logs:**
```
[Information] AnalysisWorker is starting
[Information] Dequeued analysis job. JobId: {guid}, Ticker: NVDA
[Information] Starting analysis job processing. JobId: {guid}, Ticker: NVDA
```

If not seeing these logs, ensure `AddHostedService<AnalysisWorker>()` is registered.

### Jobs stuck in Queued

**Check:**
1. Worker is running (check logs)
2. No exceptions in worker loop
3. Queue not full (capacity: 100)

### 409 Conflict on result endpoint

This is **expected** while job is still running. Wait and retry.

The response includes `retryAfterSeconds: 5` suggesting when to retry.

### Dictionary<string, object> not serializing

Ensure JSON options configured in Program.cs:
```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});
```

---

## Performance Testing

### Multiple concurrent requests:

```bash
for i in {1..10}; do
  curl -s -X POST https://localhost:7001/analysis \
    -H "Content-Type: application/json" \
    -d '{"ticker":"NVDA"}' &
done
wait
```

Expected:
- All 10 jobs should be created successfully
- Queue should handle all requests
- Worker should process them sequentially
- Check with: `GET /health` to verify system still healthy

---

## Success Criteria

? Health check returns 200
? POST /analysis returns 202 with jobId
? GET /analysis/{jobId} returns 200 with job metadata
? Job transitions: Queued ? Running ? Succeeded
? GET /analysis/{jobId}/steps returns 4 steps with artifacts
? GET /analysis/{jobId}/result returns 200 with investment report
? Report includes: company, thesis, signal, insights, sources
? All insights have type and detail
? All sources cited with mock:// references
? ScoreBreakdown shows explainable weights
