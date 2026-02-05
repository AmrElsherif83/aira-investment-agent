namespace Aira.Core.Models;

/// <summary>
/// Represents a unit of work to be processed by the background task queue.
/// </summary>
public class JobWorkItem
{
    /// <summary>
    /// Unique identifier of the analysis job to process.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// Stock ticker symbol for the analysis.
    /// </summary>
    public required string Ticker { get; set; }
}
