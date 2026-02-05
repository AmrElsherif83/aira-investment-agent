namespace Aira.Core.Models;

using Aira.Core.Models.Enums;

/// <summary>
/// Represents the execution result and artifacts from a single agent step.
/// </summary>
public class AgentStepResult
{
    /// <summary>
    /// Name of the step (e.g., "Planning", "Financial Data Gathering", "Synthesis").
    /// </summary>
    public required string StepName { get; set; }

    /// <summary>
    /// Current execution status of the step.
    /// </summary>
    public AgentStepStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the step began execution.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// Timestamp when the step completed (null if still running or not started).
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// Human-readable summary of what the step accomplished or found.
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// Dictionary of artifacts produced by the step (e.g., data snapshots, intermediate results).
    /// All values must be JSON-serializable.
    /// </summary>
    public Dictionary<string, object> Artifacts { get; set; } = new();
}
