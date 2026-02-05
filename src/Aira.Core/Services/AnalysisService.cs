namespace Aira.Core.Services;

using Aira.Core.Interfaces;
using Aira.Core.Models;
using Aira.Core.Models.Enums;
using Microsoft.Extensions.Logging;

/// <summary>
/// Coordinates analysis workflow between API, agent, and storage layers.
/// Implements CQRS-style command/query separation without heavy frameworks.
/// </summary>
public class AnalysisService : IAnalysisService
{
    private readonly IJobStore _jobStore;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IAnalysisAgent _agent;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        IJobStore jobStore,
        IBackgroundTaskQueue taskQueue,
        IAnalysisAgent agent,
        ILogger<AnalysisService> logger)
    {
        _jobStore = jobStore ?? throw new ArgumentNullException(nameof(jobStore));
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<AnalysisJob> SubmitAsync(string ticker)
    {
        // Validate ticker
        if (string.IsNullOrWhiteSpace(ticker))
        {
            throw new ArgumentException("Ticker cannot be empty or whitespace.", nameof(ticker));
        }

        // Normalize to uppercase
        ticker = ticker.Trim().ToUpperInvariant();

        // Additional validation - basic ticker format
        if (ticker.Length < 1 || ticker.Length > 10)
        {
            throw new ArgumentException(
                "Ticker must be between 1 and 10 characters.",
                nameof(ticker));
        }

        // Check for invalid characters (alphanumeric and some symbols allowed)
        if (!System.Text.RegularExpressions.Regex.IsMatch(ticker, @"^[A-Z0-9\.\-]+$"))
        {
            throw new ArgumentException(
                "Ticker contains invalid characters. Only alphanumeric, dots, and hyphens allowed.",
                nameof(ticker));
        }

        _logger.LogInformation("Submitting analysis request for ticker: {Ticker}", ticker);

        try
        {
            // Create job via job store
            var job = await _jobStore.CreateJobAsync(ticker);

            _logger.LogInformation(
                "Created analysis job. JobId: {JobId}, Ticker: {Ticker}, Status: {Status}",
                job.JobId,
                job.Ticker,
                job.Status);

            // Enqueue work item for background processing
            var workItem = new JobWorkItem
            {
                JobId = job.JobId,
                Ticker = ticker
            };

            await _taskQueue.EnqueueAsync(workItem);

            _logger.LogInformation(
                "Enqueued analysis job for background processing. JobId: {JobId}, Ticker: {Ticker}",
                job.JobId,
                ticker);

            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to submit analysis request for ticker: {Ticker}",
                ticker);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RunJobAsync(Guid jobId, string ticker, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting job execution. JobId: {JobId}, Ticker: {Ticker}",
            jobId,
            ticker);

        try
        {
            // Set job status to Running
            await _jobStore.UpdateStatusAsync(jobId, JobStatus.Running);

            _logger.LogInformation(
                "Job status updated to Running. JobId: {JobId}",
                jobId);

            // Execute analysis agent
            var (report, steps) = await _agent.ExecuteAsync(ticker, cancellationToken);

            _logger.LogInformation(
                "Analysis agent completed execution. JobId: {JobId}, Steps: {StepCount}",
                jobId,
                steps.Count);

            // Append each step to job store
            foreach (var step in steps)
            {
                await _jobStore.AppendStepAsync(jobId, step);

                _logger.LogDebug(
                    "Appended step to job. JobId: {JobId}, StepName: {StepName}, Status: {Status}",
                    jobId,
                    step.StepName,
                    step.Status);
            }

            // Save final report
            await _jobStore.SaveResultAsync(jobId, report);

            _logger.LogInformation(
                "Saved final report to job. JobId: {JobId}, Signal: {Signal}, Confidence: {Confidence:P0}",
                jobId,
                report.Signal,
                report.Confidence);

            // Set job status to Succeeded
            await _jobStore.UpdateStatusAsync(jobId, JobStatus.Succeeded);

            _logger.LogInformation(
                "Job completed successfully. JobId: {JobId}, Ticker: {Ticker}",
                jobId,
                ticker);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful cancellation
            _logger.LogWarning(
                "Job execution cancelled. JobId: {JobId}, Ticker: {Ticker}",
                jobId,
                ticker);

            await UpdateJobToFailedAsync(
                jobId,
                ticker,
                "Job execution was cancelled due to shutdown or timeout.");

            throw; // Re-throw to signal cancellation to caller
        }
        catch (Exception ex)
        {
            // Job execution failed
            _logger.LogError(
                ex,
                "Job execution failed. JobId: {JobId}, Ticker: {Ticker}, Error: {ErrorMessage}",
                jobId,
                ticker,
                ex.Message);

            await UpdateJobToFailedAsync(jobId, ticker, ex);
        }
    }

    /// <summary>
    /// Updates job to Failed status with error message and creates a failure step.
    /// </summary>
    private async Task UpdateJobToFailedAsync(Guid jobId, string ticker, Exception ex)
    {
        var errorMessage = GetSafeErrorMessage(ex);
        await UpdateJobToFailedAsync(jobId, ticker, errorMessage);

        // Create a failed step with exception details
        var failedStep = new AgentStepResult
        {
            StepName = "Job Execution",
            Status = AgentStepStatus.Failed,
            StartedAt = DateTimeOffset.UtcNow,
            FinishedAt = DateTimeOffset.UtcNow,
            Summary = $"Job execution failed: {ex.GetType().Name}",
            Artifacts = new Dictionary<string, object>
            {
                ["error_type"] = ex.GetType().Name,
                ["error_message"] = ex.Message,
                ["stack_trace_summary"] = GetStackTraceSummary(ex)
            }
        };

        try
        {
            await _jobStore.AppendStepAsync(jobId, failedStep);
        }
        catch (Exception appendEx)
        {
            _logger.LogError(
                appendEx,
                "Failed to append failure step to job. JobId: {JobId}",
                jobId);
        }
    }

    /// <summary>
    /// Updates job to Failed status with error message.
    /// </summary>
    private async Task UpdateJobToFailedAsync(Guid jobId, string ticker, string errorMessage)
    {
        try
        {
            await _jobStore.UpdateStatusAsync(jobId, JobStatus.Failed, errorMessage);

            _logger.LogInformation(
                "Job status updated to Failed. JobId: {JobId}, Error: {ErrorMessage}",
                jobId,
                errorMessage);
        }
        catch (Exception updateEx)
        {
            _logger.LogError(
                updateEx,
                "Failed to update job status to Failed. JobId: {JobId}",
                jobId);
        }
    }

    /// <summary>
    /// Extracts a safe, truncated error message from exception.
    /// </summary>
    private static string GetSafeErrorMessage(Exception ex)
    {
        const int maxLength = 500;

        var message = $"{ex.GetType().Name}: {ex.Message}";

        if (message.Length > maxLength)
        {
            message = message.Substring(0, maxLength) + "... (truncated)";
        }

        return message;
    }

    /// <summary>
    /// Extracts first few lines of stack trace for debugging.
    /// </summary>
    private static string GetStackTraceSummary(Exception ex)
    {
        if (string.IsNullOrEmpty(ex.StackTrace))
        {
            return "No stack trace available";
        }

        var lines = ex.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var summary = string.Join(" | ", lines.Take(3));

        const int maxLength = 300;
        if (summary.Length > maxLength)
        {
            summary = summary.Substring(0, maxLength) + "...";
        }

        return summary;
    }
}
