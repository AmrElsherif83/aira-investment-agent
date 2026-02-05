namespace Aira.Api.Endpoints;

using Aira.Core.Interfaces;
using Aira.Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Extension methods to register analysis endpoints.
/// </summary>
public static class AnalysisEndpoints
{
    /// <summary>
    /// Maps all analysis-related endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/analysis")
            .WithTags("Analysis")
            .WithOpenApi();

        group.MapPost("", SubmitAnalysisAsync)
            .WithName("SubmitAnalysis")
            .WithSummary("Submit a new investment analysis request")
            .Produces<SubmitAnalysisResponse>(StatusCodes.Status202Accepted)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("{jobId:guid}", GetAnalysisJobAsync)
            .WithName("GetAnalysisJob")
            .WithSummary("Get analysis job status and metadata")
            .Produces<AnalysisJobResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{jobId:guid}/steps", GetAnalysisStepsAsync)
            .WithName("GetAnalysisSteps")
            .WithSummary("Get detailed step-by-step execution trace")
            .Produces<GetStepsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{jobId:guid}/result", GetAnalysisResultAsync)
            .WithName("GetAnalysisResult")
            .WithSummary("Get final investment report (if completed)")
            .Produces<GetResultResponse>(StatusCodes.Status200OK)
            .Produces<ConflictResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// POST /analysis - Submit new analysis request.
    /// </summary>
    private static async Task<IResult> SubmitAnalysisAsync(
        [FromBody] SubmitAnalysisRequest request,
        [FromServices] IAnalysisService analysisService,
        HttpContext context)
    {
        try
        {
            // Submit analysis
            var job = await analysisService.SubmitAsync(request.Ticker);

            // Build URLs for client
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
            var statusUrl = $"{baseUrl}/analysis/{job.JobId}";
            var stepsUrl = $"{baseUrl}/analysis/{job.JobId}/steps";
            var resultUrl = $"{baseUrl}/analysis/{job.JobId}/result";

            var response = new SubmitAnalysisResponse
            {
                JobId = job.JobId,
                Ticker = job.Ticker,
                Status = job.Status.ToString(),
                SubmittedAt = job.CreatedAt,
                StatusUrl = statusUrl,
                StepsUrl = stepsUrl,
                ResultUrl = resultUrl
            };

            return Results.Accepted(statusUrl, response);
        }
        catch (ArgumentException ex)
        {
            // Validation error
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ticker"] = new[] { ex.Message }
            });
        }
        catch (InvalidOperationException ex)
        {
            // Queue full or similar operational error
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Service Unavailable");
        }
    }

    /// <summary>
    /// GET /analysis/{jobId} - Get job metadata and status.
    /// </summary>
    private static async Task<IResult> GetAnalysisJobAsync(
        Guid jobId,
        [FromServices] IJobStore jobStore)
    {
        var job = await jobStore.GetJobAsync(jobId);

        if (job == null)
        {
            return Results.NotFound(new { error = $"Job {jobId} not found." });
        }

        var response = new AnalysisJobResponse
        {
            JobId = job.JobId,
            Ticker = job.Ticker,
            Status = job.Status.ToString(),
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            FinishedAt = job.FinishedAt,
            Error = job.Error,
            StepCount = job.Steps.Count,
            HasResult = job.Result != null
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// GET /analysis/{jobId}/steps - Get step execution trace.
    /// </summary>
    private static async Task<IResult> GetAnalysisStepsAsync(
        Guid jobId,
        [FromServices] IJobStore jobStore)
    {
        var job = await jobStore.GetJobAsync(jobId);

        if (job == null)
        {
            return Results.NotFound(new { error = $"Job {jobId} not found." });
        }

        var response = new GetStepsResponse
        {
            JobId = job.JobId,
            Ticker = job.Ticker,
            Status = job.Status.ToString(),
            Steps = job.Steps
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// GET /analysis/{jobId}/result - Get final investment report.
    /// </summary>
    private static async Task<IResult> GetAnalysisResultAsync(
        Guid jobId,
        [FromServices] IJobStore jobStore)
    {
        var job = await jobStore.GetJobAsync(jobId);

        if (job == null)
        {
            return Results.NotFound(new { error = $"Job {jobId} not found." });
        }

        // Check job status
        if (job.Status == JobStatus.Queued || job.Status == JobStatus.Running)
        {
            // Job not complete yet - return 409 Conflict
            var conflictResponse = new ConflictResponse
            {
                JobId = job.JobId,
                Status = job.Status.ToString(),
                Message = $"Analysis is still in progress. Current status: {job.Status}",
                RetryAfterSeconds = 5
            };

            return Results.Conflict(conflictResponse);
        }

        if (job.Status == JobStatus.Failed)
        {
            // Job failed - return 500 with error details
            var errorResponse = new ErrorResponse
            {
                JobId = job.JobId,
                Status = job.Status.ToString(),
                Error = job.Error ?? "Job failed with unknown error",
                FailedAt = job.FinishedAt
            };

            return Results.Problem(
                detail: errorResponse.Error,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Analysis Failed",
                instance: $"/analysis/{jobId}/result",
                extensions: new Dictionary<string, object?>
                {
                    ["jobId"] = errorResponse.JobId,
                    ["failedAt"] = errorResponse.FailedAt
                });
        }

        // Job succeeded - return result
        if (job.Result == null)
        {
            // Unexpected: job succeeded but no result
            return Results.Problem(
                detail: "Job marked as succeeded but result is missing.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Error");
        }

        var response = new GetResultResponse
        {
            JobId = job.JobId,
            Ticker = job.Ticker,
            Status = job.Status.ToString(),
            CompletedAt = job.FinishedAt,
            Result = job.Result
        };

        return Results.Ok(response);
    }

    // ===== Request/Response DTOs =====

    public record SubmitAnalysisRequest
    {
        public required string Ticker { get; init; }
    }

    public record SubmitAnalysisResponse
    {
        public required Guid JobId { get; init; }
        public required string Ticker { get; init; }
        public required string Status { get; init; }
        public required DateTimeOffset SubmittedAt { get; init; }
        public required string StatusUrl { get; init; }
        public required string StepsUrl { get; init; }
        public required string ResultUrl { get; init; }
    }

    public record AnalysisJobResponse
    {
        public required Guid JobId { get; init; }
        public required string Ticker { get; init; }
        public required string Status { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? StartedAt { get; init; }
        public DateTimeOffset? FinishedAt { get; init; }
        public string? Error { get; init; }
        public required int StepCount { get; init; }
        public required bool HasResult { get; init; }
    }

    public record GetStepsResponse
    {
        public required Guid JobId { get; init; }
        public required string Ticker { get; init; }
        public required string Status { get; init; }
        public required List<Core.Models.AgentStepResult> Steps { get; init; }
    }

    public record GetResultResponse
    {
        public required Guid JobId { get; init; }
        public required string Ticker { get; init; }
        public required string Status { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
        public required Core.Models.InvestmentReport Result { get; init; }
    }

    public record ConflictResponse
    {
        public required Guid JobId { get; init; }
        public required string Status { get; init; }
        public required string Message { get; init; }
        public int RetryAfterSeconds { get; init; }
    }

    public record ErrorResponse
    {
        public required Guid JobId { get; init; }
        public required string Status { get; init; }
        public required string Error { get; init; }
        public DateTimeOffset? FailedAt { get; init; }
    }
}
