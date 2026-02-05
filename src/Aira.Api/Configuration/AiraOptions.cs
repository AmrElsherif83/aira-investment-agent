namespace Aira.Api.Configuration;

/// <summary>
/// Configuration options for the AIRA application.
/// Binds to the "Aira" section in appsettings.json.
/// Can be overridden with environment variables using the format: Aira__PropertyName
/// </summary>
public class AiraOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Aira";

    /// <summary>
    /// Default stock ticker to use for testing or when not specified.
    /// Environment variable: Aira__DefaultTicker
    /// </summary>
    public string DefaultTicker { get; set; } = "NVDA";

    /// <summary>
    /// Whether to include verbose step artifacts in API responses.
    /// Useful for debugging but may increase response size.
    /// Environment variable: Aira__EnableVerboseStepArtifacts
    /// </summary>
    public bool EnableVerboseStepArtifacts { get; set; } = true;

    /// <summary>
    /// Maximum capacity of the background task queue.
    /// Requests exceeding this limit will receive 503 Service Unavailable.
    /// Environment variable: Aira__QueueCapacity
    /// </summary>
    public int QueueCapacity { get; set; } = 100;

    /// <summary>
    /// Polling interval in seconds for the background worker to check for new jobs.
    /// Lower values provide faster processing but higher CPU usage.
    /// Environment variable: Aira__WorkerPollingIntervalSeconds
    /// </summary>
    public int WorkerPollingIntervalSeconds { get; set; } = 2;

    /// <summary>
    /// Maximum number of jobs the worker can process concurrently.
    /// Currently not implemented (worker processes sequentially).
    /// Environment variable: Aira__MaxConcurrentJobs
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = 1;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <returns>Validation result with error messages if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(DefaultTicker))
        {
            return (false, "DefaultTicker cannot be empty.");
        }

        if (QueueCapacity < 1 || QueueCapacity > 10000)
        {
            return (false, "QueueCapacity must be between 1 and 10000.");
        }

        if (WorkerPollingIntervalSeconds < 1 || WorkerPollingIntervalSeconds > 60)
        {
            return (false, "WorkerPollingIntervalSeconds must be between 1 and 60.");
        }

        if (MaxConcurrentJobs < 1 || MaxConcurrentJobs > 100)
        {
            return (false, "MaxConcurrentJobs must be between 1 and 100.");
        }

        return (true, null);
    }
}
