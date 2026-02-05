namespace Aira.Core.Models;

using Aira.Core.Models.Enums;

/// <summary>
/// Represents a complete analysis job including its status, steps, and final result.
/// </summary>
public class AnalysisJob
{
    /// <summary>
    /// Unique identifier for the job.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// Stock ticker being analyzed.
    /// </summary>
    public required string Ticker { get; set; }

    /// <summary>
    /// Current lifecycle status of the job.
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the job was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the job started processing (null if not started).
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Timestamp when the job finished processing (null if still running or queued).
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// Error message if the job failed (null if successful or still running).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// List of individual step results showing the agent's multi-step execution.
    /// </summary>
    public List<AgentStepResult> Steps { get; set; } = new();

    /// <summary>
    /// Final investment report (null until job succeeds).
    /// </summary>
    public InvestmentReport? Result { get; set; }
}
