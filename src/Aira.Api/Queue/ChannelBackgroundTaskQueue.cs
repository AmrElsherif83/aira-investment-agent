namespace Aira.Api.Queue;

using System.Threading.Channels;
using Aira.Core.Interfaces;
using Aira.Core.Models;

/// <summary>
/// Thread-safe background task queue using System.Threading.Channels.
/// Provides bounded FIFO queue for job work items with clear error handling.
/// </summary>
public class ChannelBackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<JobWorkItem> _queue;
    private readonly int _capacity;

    /// <summary>
    /// Initializes a new instance of the ChannelBackgroundTaskQueue with specified capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of items that can be queued. Default is 100.</param>
    public ChannelBackgroundTaskQueue(int capacity = 100)
    {
        _capacity = capacity;

        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _queue = Channel.CreateBounded<JobWorkItem>(options);
    }

    /// <inheritdoc/>
    public async Task EnqueueAsync(JobWorkItem workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem), "Work item cannot be null.");
        }

        // Try to write with a timeout to detect if queue is full
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            await _queue.Writer.WriteAsync(workItem, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Background task queue is full (capacity: {_capacity}). " +
                "Unable to enqueue new work item. Please try again later.");
        }
    }

    /// <inheritdoc/>
    public async Task<JobWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
