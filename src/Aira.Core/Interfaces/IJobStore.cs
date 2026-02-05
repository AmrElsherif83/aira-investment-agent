namespace Aira.Core.Interfaces;

using Aira.Core.Models;
using Aira.Core.Models.Enums;

/// <summary>
/// Provides storage and retrieval operations for analysis jobs.
/// No generic repository pattern - specific methods for the domain.
/// </summary>
public interface IJobStore
{
    /// <summary>
    /// Creates a new analysis job for the specified ticker.
    /// </summary>
    /// <param name="ticker">Stock ticker symbol to analyze.</param>
    /// <returns>The newly created analysis job.</returns>
    Task<AnalysisJob> CreateJobAsync(string ticker);

    /// <summary>
    /// Retrieves an analysis job by its unique identifier.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>The analysis job if found; otherwise, null.</returns>
    Task<AnalysisJob?> GetJobAsync(Guid jobId);

    /// <summary>
    /// Updates the status of an analysis job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="status">The new status.</param>
    /// <param name="error">Optional error message if the job failed.</param>
    Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null);

    /// <summary>
    /// Appends a step result to the job's execution history.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="step">The step result to append.</param>
    Task AppendStepAsync(Guid jobId, AgentStepResult step);

    /// <summary>
    /// Saves the final investment report for a completed job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="report">The final investment report.</param>
    Task SaveResultAsync(Guid jobId, InvestmentReport report);
}
