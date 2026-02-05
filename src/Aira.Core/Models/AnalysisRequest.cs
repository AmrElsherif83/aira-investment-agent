namespace Aira.Core.Models;

/// <summary>
/// Request model for submitting a new investment analysis job.
/// </summary>
public class AnalysisRequest
{
    /// <summary>
    /// Stock ticker symbol (required, e.g., "NVDA", "AAPL").
    /// </summary>
    public required string Ticker { get; set; }

    /// <summary>
    /// Company name (optional, e.g., "NVIDIA"). If not provided, may be resolved from ticker.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Timestamp when the analysis was requested.
    /// </summary>
    public DateTimeOffset RequestedAt { get; set; }
}
