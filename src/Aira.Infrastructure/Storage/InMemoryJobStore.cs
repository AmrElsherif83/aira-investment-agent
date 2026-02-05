namespace Aira.Infrastructure.Storage;

using System.Collections.Concurrent;
using Aira.Core.Interfaces;
using Aira.Core.Models;
using Aira.Core.Models.Enums;

/// <summary>
/// Thread-safe in-memory implementation of job storage.
/// Uses ConcurrentDictionary with lock-per-job for complex updates.
/// </summary>
public class InMemoryJobStore : IJobStore
{
    private readonly ConcurrentDictionary<Guid, AnalysisJob> _jobs = new();
    private readonly ConcurrentDictionary<Guid, object> _locks = new();

    /// <inheritdoc/>
    public Task<AnalysisJob> CreateJobAsync(string ticker)
    {
        var job = new AnalysisJob
        {
            JobId = Guid.NewGuid(),
            Ticker = ticker,
            Status = JobStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow,
            Steps = new List<AgentStepResult>()
        };

        if (!_jobs.TryAdd(job.JobId, job))
        {
            throw new InvalidOperationException($"Job with ID {job.JobId} already exists.");
        }

        _locks.TryAdd(job.JobId, new object());

        return Task.FromResult(job);
    }

    /// <inheritdoc/>
    public Task<AnalysisJob?> GetJobAsync(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    /// <inheritdoc/>
    public Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            throw new InvalidOperationException($"Job {jobId} not found.");
        }

        if (!_locks.TryGetValue(jobId, out var lockObj))
        {
            lockObj = new object();
            _locks.TryAdd(jobId, lockObj);
        }

        lock (lockObj)
        {
            job.Status = status;
            job.Error = error;

            // Update timestamps based on status transitions
            if (status == JobStatus.Running && job.StartedAt == null)
            {
                job.StartedAt = DateTimeOffset.UtcNow;
            }
            else if (status == JobStatus.Succeeded || status == JobStatus.Failed)
            {
                job.FinishedAt = DateTimeOffset.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task AppendStepAsync(Guid jobId, AgentStepResult step)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            throw new InvalidOperationException($"Job {jobId} not found.");
        }

        if (!_locks.TryGetValue(jobId, out var lockObj))
        {
            lockObj = new object();
            _locks.TryAdd(jobId, lockObj);
        }

        lock (lockObj)
        {
            // Ensure steps are appended in order
            job.Steps.Add(step);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SaveResultAsync(Guid jobId, InvestmentReport report)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            throw new InvalidOperationException($"Job {jobId} not found.");
        }

        if (!_locks.TryGetValue(jobId, out var lockObj))
        {
            lockObj = new object();
            _locks.TryAdd(jobId, lockObj);
        }

        lock (lockObj)
        {
            job.Result = report;
        }

        return Task.CompletedTask;
    }
}
