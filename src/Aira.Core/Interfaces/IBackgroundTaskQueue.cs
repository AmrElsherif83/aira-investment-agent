namespace Aira.Core.Interfaces;

using Aira.Core.Models;

/// <summary>
/// Provides a queue for background processing of analysis jobs.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Enqueues a work item for background processing.
    /// </summary>
    /// <param name="workItem">The work item containing job details.</param>
    Task EnqueueAsync(JobWorkItem workItem);

    /// <summary>
    /// Dequeues the next work item for processing.
    /// Blocks until a work item is available or cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop waiting.</param>
    /// <returns>The next work item to process.</returns>
    Task<JobWorkItem> DequeueAsync(CancellationToken cancellationToken);
}
