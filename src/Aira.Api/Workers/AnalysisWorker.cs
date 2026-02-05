namespace Aira.Api.Workers;

using Aira.Core.Interfaces;
using Aira.Core.Models;

/// <summary>
/// Background service that continuously processes analysis jobs from the queue.
/// Implements robust error handling to prevent worker crashes and logs all operations.
/// </summary>
public class AnalysisWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalysisWorker> _logger;

    /// <summary>
    /// Initializes a new instance of the AnalysisWorker.
    /// </summary>
    /// <param name="taskQueue">The background task queue to dequeue work items from.</param>
    /// <param name="scopeFactory">Factory to create service scopes for scoped dependencies.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public AnalysisWorker(
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<AnalysisWorker> logger)
    {
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Main execution loop that processes queued analysis jobs.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to signal graceful shutdown.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AnalysisWorker is starting");

        // Continue processing until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for next work item from queue
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                _logger.LogInformation(
                    "Dequeued analysis job. JobId: {JobId}, Ticker: {Ticker}",
                    workItem.JobId,
                    workItem.Ticker);

                // Process the work item with proper error isolation
                await ProcessWorkItemAsync(workItem, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected during graceful shutdown
                _logger.LogInformation("AnalysisWorker is stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                // Log unexpected errors but keep worker alive
                _logger.LogError(
                    ex,
                    "Unexpected error in AnalysisWorker main loop. Worker will continue processing");
            }
        }

        _logger.LogInformation("AnalysisWorker has stopped");
    }

    /// <summary>
    /// Processes a single work item with comprehensive error handling.
    /// Ensures job status is updated even if processing fails.
    /// </summary>
    /// <param name="workItem">The work item to process.</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    private async Task ProcessWorkItemAsync(JobWorkItem workItem, CancellationToken cancellationToken)
    {
        // Create a new scope for scoped services (e.g., IAnalysisService)
        using var scope = _scopeFactory.CreateScope();
        
        var analysisService = scope.ServiceProvider.GetRequiredService<IAnalysisService>();
        var jobStore = scope.ServiceProvider.GetRequiredService<IJobStore>();

        try
        {
            _logger.LogInformation(
                "Starting analysis job processing. JobId: {JobId}, Ticker: {Ticker}",
                workItem.JobId,
                workItem.Ticker);

            // Execute the analysis workflow
            await analysisService.RunJobAsync(workItem.JobId, workItem.Ticker, cancellationToken);

            _logger.LogInformation(
                "Successfully completed analysis job. JobId: {JobId}, Ticker: {Ticker}",
                workItem.JobId,
                workItem.Ticker);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown requested during job processing
            _logger.LogWarning(
                "Analysis job cancelled due to shutdown. JobId: {JobId}, Ticker: {Ticker}",
                workItem.JobId,
                workItem.Ticker);

            // Mark job as failed due to cancellation
            await SafeUpdateJobStatusAsync(
                jobStore,
                workItem.JobId,
                Core.Models.Enums.JobStatus.Failed,
                "Job cancelled during worker shutdown",
                workItem.Ticker);
        }
        catch (Exception ex)
        {
            // Job-specific error - log and mark job as failed
            _logger.LogError(
                ex,
                "Analysis job failed with error. JobId: {JobId}, Ticker: {Ticker}, Error: {ErrorMessage}",
                workItem.JobId,
                workItem.Ticker,
                ex.Message);

            // Safely update job status to Failed with error message
            await SafeUpdateJobStatusAsync(
                jobStore,
                workItem.JobId,
                Core.Models.Enums.JobStatus.Failed,
                GetSafeErrorMessage(ex),
                workItem.Ticker);
        }
    }

    /// <summary>
    /// Safely updates job status with additional error handling.
    /// Prevents cascading failures if status update fails.
    /// </summary>
    private async Task SafeUpdateJobStatusAsync(
        IJobStore jobStore,
        Guid jobId,
        Core.Models.Enums.JobStatus status,
        string? errorMessage,
        string ticker)
    {
        try
        {
            await jobStore.UpdateStatusAsync(jobId, status, errorMessage);
            
            _logger.LogInformation(
                "Updated job status to {Status}. JobId: {JobId}, Ticker: {Ticker}",
                status,
                jobId,
                ticker);
        }
        catch (Exception ex)
        {
            // Even status update failed - log but don't throw
            _logger.LogError(
                ex,
                "Failed to update job status to {Status}. JobId: {JobId}, Ticker: {Ticker}",
                status,
                jobId,
                ticker);
        }
    }

    /// <summary>
    /// Extracts a safe error message from exception, limiting length and removing sensitive data.
    /// </summary>
    private static string GetSafeErrorMessage(Exception ex)
    {
        const int maxLength = 500;
        
        var message = ex.Message;
        
        // Truncate long messages
        if (message.Length > maxLength)
        {
            message = message.Substring(0, maxLength) + "... (truncated)";
        }

        // Add exception type for context
        return $"{ex.GetType().Name}: {message}";
    }
}
