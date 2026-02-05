namespace Aira.Infrastructure.Queue;

using System.Threading.Channels;
using Aira.Core.Interfaces;
using Aira.Core.Models;

/// <summary>
/// Thread-safe background task queue using System.Threading.Channels.
/// Provides bounded FIFO queue for job work items.
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<JobWorkItem> _queue;

    /// <summary>
    /// Initializes a new instance of the BackgroundTaskQueue with specified capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of items that can be queued. Default is 100.</param>
    public BackgroundTaskQueue(int capacity = 100)
    {
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
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
    }

    /// <inheritdoc/>
    public async Task<JobWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
