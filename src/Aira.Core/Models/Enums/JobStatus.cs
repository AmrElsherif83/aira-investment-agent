namespace Aira.Core.Models.Enums;

/// <summary>
/// Represents the lifecycle status of an analysis job.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job is queued and waiting to be processed.
    /// </summary>
    Queued,

    /// <summary>
    /// Job is currently being processed by the background worker.
    /// </summary>
    Running,

    /// <summary>
    /// Job completed successfully with results.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Job failed due to an error during processing.
    /// </summary>
    Failed
}
