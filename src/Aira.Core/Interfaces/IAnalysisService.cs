namespace Aira.Core.Interfaces;

using Aira.Core.Models;

/// <summary>
/// Coordinates the submission and execution of analysis jobs.
/// Entry point for analysis workflow.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Submits a new analysis request for the specified ticker.
    /// Creates a job and enqueues it for background processing.
    /// </summary>
    /// <param name="ticker">Stock ticker symbol to analyze.</param>
    /// <returns>The created analysis job with initial status.</returns>
    Task<AnalysisJob> SubmitAsync(string ticker);

    /// <summary>
    /// Executes the analysis workflow for a specific job.
    /// Called by the background worker to process queued jobs.
    /// </summary>
    /// <param name="jobId">The job identifier to process.</param>
    /// <param name="ticker">The stock ticker for the job.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    Task RunJobAsync(Guid jobId, string ticker, CancellationToken cancellationToken);
}
