# AIRA Configuration Guide

## Overview

The AIRA application uses strongly-typed configuration binding with validation. Configuration can be provided through:
1. `appsettings.json` (production defaults)
2. `appsettings.Development.json` (development overrides)
3. Environment variables (deployment overrides)

---

## Configuration Class: AiraOptions

All configuration is bound to the `AiraOptions` class:

```csharp
public class AiraOptions
{
    public string DefaultTicker { get; set; } = "NVDA";
    public bool EnableVerboseStepArtifacts { get; set; } = true;
    public int QueueCapacity { get; set; } = 100;
    public int WorkerPollingIntervalSeconds { get; set; } = 2;
    public int MaxConcurrentJobs { get; set; } = 1;
}
```

---

## Configuration Properties

### DefaultTicker
- **Type:** `string`
- **Default:** `"NVDA"`
- **Description:** Default stock ticker for testing or when not specified
- **Environment Variable:** `Aira__DefaultTicker`
- **Example:** `"AAPL"`, `"MSFT"`, `"TSLA"`

### EnableVerboseStepArtifacts
- **Type:** `bool`
- **Default:** `true`
- **Description:** Whether to include verbose step artifacts in API responses
- **Environment Variable:** `Aira__EnableVerboseStepArtifacts`
- **Use Case:** Set to `false` in production to reduce response size
- **Example:** `true` (development), `false` (production)

### QueueCapacity
- **Type:** `int`
- **Default:** `100` (production), `50` (development)
- **Description:** Maximum capacity of the background task queue
- **Environment Variable:** `Aira__QueueCapacity`
- **Valid Range:** 1 to 10,000
- **Behavior:** Requests exceeding this limit receive `503 Service Unavailable`
- **Example:** `100` (production), `50` (development/testing)

### WorkerPollingIntervalSeconds
- **Type:** `int`
- **Default:** `2` (production), `1` (development)
- **Description:** Polling interval for background worker to check for new jobs
- **Environment Variable:** `Aira__WorkerPollingIntervalSeconds`
- **Valid Range:** 1 to 60 seconds
- **Trade-off:** Lower = faster processing but higher CPU; Higher = slower but more efficient
- **Example:** `2` (production), `1` (development)

### MaxConcurrentJobs
- **Type:** `int`
- **Default:** `1`
- **Description:** Maximum jobs worker can process concurrently
- **Environment Variable:** `Aira__MaxConcurrentJobs`
- **Valid Range:** 1 to 100
- **Note:** Currently not implemented (worker processes sequentially)
- **Future Use:** For parallel job processing

---

## Configuration Files

### appsettings.json (Production)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Aira": "Information",
      "Aira.Core.Services.AnalysisService": "Information",
      "Aira.Api.Workers.AnalysisWorker": "Information"
    }
  },
  "AllowedHosts": "*",
  "Aira": {
    "DefaultTicker": "NVDA",
    "EnableVerboseStepArtifacts": true,
    "QueueCapacity": 100,
    "WorkerPollingIntervalSeconds": 2,
    "MaxConcurrentJobs": 1
  }
}
```

### appsettings.Development.json (Development)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Aira": "Debug",
      "Aira.Core.Services.AnalysisService": "Debug",
      "Aira.Api.Workers.AnalysisWorker": "Debug",
      "Aira.Core.Agent.AnalysisAgent": "Debug"
    }
  },
  "Aira": {
    "DefaultTicker": "NVDA",
    "EnableVerboseStepArtifacts": true,
    "QueueCapacity": 50,
    "WorkerPollingIntervalSeconds": 1,
    "MaxConcurrentJobs": 1
  }
}
```

**Key Differences:**
- Development has more verbose logging (Debug level)
- Development has smaller queue (50 vs 100)
- Development has faster polling (1s vs 2s)

---

## Environment Variable Overrides

Configuration can be overridden using environment variables with double-underscore (`__`) separator:

### Format
```
Aira__PropertyName=value
```

### Examples

**Linux/macOS:**
```bash
export Aira__DefaultTicker="AAPL"
export Aira__QueueCapacity=200
export Aira__EnableVerboseStepArtifacts=false
export Aira__WorkerPollingIntervalSeconds=5

dotnet run
```

**Windows (PowerShell):**
```powershell
$env:Aira__DefaultTicker = "AAPL"
$env:Aira__QueueCapacity = 200
$env:Aira__EnableVerboseStepArtifacts = $false
$env:Aira__WorkerPollingIntervalSeconds = 5

dotnet run
```

**Windows (Command Prompt):**
```cmd
set Aira__DefaultTicker=AAPL
set Aira__QueueCapacity=200
set Aira__EnableVerboseStepArtifacts=false
set Aira__WorkerPollingIntervalSeconds=5

dotnet run
```

**Docker:**
```dockerfile
ENV Aira__DefaultTicker=AAPL \
    Aira__QueueCapacity=200 \
    Aira__EnableVerboseStepArtifacts=false \
    Aira__WorkerPollingIntervalSeconds=5
```

**Azure App Service:**
```
Application Settings:
  Aira__DefaultTicker = AAPL
  Aira__QueueCapacity = 200
  Aira__EnableVerboseStepArtifacts = false
  Aira__WorkerPollingIntervalSeconds = 5
```

---

## Configuration Precedence

Configuration sources are applied in this order (later overrides earlier):

1. **Default values** in `AiraOptions` class
2. **appsettings.json**
3. **appsettings.{Environment}.json** (e.g., `appsettings.Development.json`)
4. **Environment variables**
5. **Command-line arguments** (if configured)

### Example Precedence

Given:
- `AiraOptions.DefaultTicker = "NVDA"` (class default)
- `appsettings.json` has `"DefaultTicker": "NVDA"`
- `appsettings.Development.json` has no `DefaultTicker` override
- Environment variable: `Aira__DefaultTicker=TSLA`

**Result:** `DefaultTicker = "TSLA"` (environment variable wins)

---

## Validation

The `AiraOptions` class includes built-in validation:

```csharp
public (bool IsValid, string? ErrorMessage) Validate()
{
    // Validates:
    // - DefaultTicker is not empty
    // - QueueCapacity is between 1 and 10,000
    // - WorkerPollingIntervalSeconds is between 1 and 60
    // - MaxConcurrentJobs is between 1 and 100
}
```

**Application Startup Behavior:**
- Configuration is validated during startup
- Invalid configuration throws `InvalidOperationException`
- Application will not start if validation fails

**Example Error:**
```
Unhandled exception. System.InvalidOperationException: 
Invalid configuration: QueueCapacity must be between 1 and 10000.
```

---

## Accessing Configuration in Code

Configuration is registered as a singleton and can be injected:

```csharp
public class MyService
{
    private readonly AiraOptions _options;

    public MyService(AiraOptions options)
    {
        _options = options;
    }

    public void DoWork()
    {
        var ticker = _options.DefaultTicker;
        var queueSize = _options.QueueCapacity;
        // Use configuration...
    }
}
```

---

## Startup Logging

Configuration values are logged on application startup:

```
info: Program[0]
      AIRA Configuration loaded:
info: Program[0]
        DefaultTicker: NVDA
info: Program[0]
        EnableVerboseStepArtifacts: True
info: Program[0]
        QueueCapacity: 100
info: Program[0]
        WorkerPollingIntervalSeconds: 2
info: Program[0]
        MaxConcurrentJobs: 1
```

**Troubleshooting:**
- Check logs to verify configuration loaded correctly
- Confirm environment variables are being applied
- Validate configuration values are within acceptable ranges

---

## Configuration Usage in Application

### Background Task Queue

Queue capacity is configured from `AiraOptions`:

```csharp
builder.Services.AddSingleton<IBackgroundTaskQueue>(sp => 
    new ChannelBackgroundTaskQueue(airaOptions.QueueCapacity));
```

### Verbose Artifacts (Future Enhancement)

The `EnableVerboseStepArtifacts` flag can be used to control artifact verbosity:

```csharp
if (!options.EnableVerboseStepArtifacts)
{
    // Strip or minimize artifacts before returning
    step.Artifacts = new Dictionary<string, object>
    {
        ["summary_only"] = step.Summary
    };
}
```

---

## Security Considerations

### ? No Secrets in Configuration

The current configuration **does not contain any secrets**:
- No API keys
- No connection strings
- No passwords
- No certificates

All data providers are mocks with deterministic data.

### ?? If Adding Secrets in Future

**DO NOT store secrets in appsettings.json or source control.**

**Use:**
1. **Environment variables** for deployment-specific secrets
2. **Azure Key Vault** for Azure deployments
3. **User Secrets** (`dotnet user-secrets`) for local development
4. **Kubernetes Secrets** for K8s deployments

**Example (if adding API key in future):**

```bash
# DO NOT put in appsettings.json
export Aira__ExternalApiKey="sk-xyz123..."

# Or use User Secrets for development
dotnet user-secrets set "Aira:ExternalApiKey" "sk-xyz123..."
```

---

## Recommended Configuration by Environment

### Development
```json
{
  "Aira": {
    "DefaultTicker": "NVDA",
    "EnableVerboseStepArtifacts": true,
    "QueueCapacity": 50,
    "WorkerPollingIntervalSeconds": 1,
    "MaxConcurrentJobs": 1
  }
}
```

### Staging
```json
{
  "Aira": {
    "DefaultTicker": "NVDA",
    "EnableVerboseStepArtifacts": true,
    "QueueCapacity": 100,
    "WorkerPollingIntervalSeconds": 2,
    "MaxConcurrentJobs": 1
  }
}
```

### Production
```json
{
  "Aira": {
    "DefaultTicker": "NVDA",
    "EnableVerboseStepArtifacts": false,
    "QueueCapacity": 200,
    "WorkerPollingIntervalSeconds": 5,
    "MaxConcurrentJobs": 1
  }
}
```

**Rationale:**
- Production has larger queue for higher load
- Production has slower polling to reduce CPU
- Production disables verbose artifacts to reduce bandwidth

---

## Troubleshooting

### Configuration Not Loading

**Check:**
1. JSON syntax is valid (use JSON validator)
2. Property names match exactly (case-sensitive)
3. Environment variables use double-underscore format
4. Application is running in expected environment

**Verify loaded configuration:**
```csharp
// Add temporary endpoint to inspect configuration
app.MapGet("/debug/config", (AiraOptions options) => 
    Results.Ok(options));
```

### Environment Variables Not Working

**Common Issues:**
1. Using single underscore instead of double (`Aira_QueueCapacity` ?)
2. Not restarting application after setting env var
3. Wrong shell/terminal (need to set in same session)

**Test:**
```bash
# Set and verify in same command
Aira__DefaultTicker=TEST dotnet run
```

### Validation Errors

**Example Error:**
```
InvalidOperationException: Invalid configuration: QueueCapacity must be between 1 and 10000.
```

**Fix:**
1. Check configuration values are within valid ranges
2. Update appsettings.json or environment variables
3. Restart application

---

## Summary

? **Configuration bound to `AiraOptions` class**
? **Supports appsettings.json, appsettings.{Environment}.json, and environment variables**
? **No secrets in configuration files**
? **Validation at startup**
? **Startup logging for verification**
? **Queue capacity configurable**
? **Environment-specific defaults**

For questions or issues, check startup logs and verify environment variables are set correctly.
