using System.Threading.Channels;

using AEL.Core.Extensions;

using Microsoft.Extensions.Logging;

namespace AEL.Core;

/// <summary>
/// Represents an asynchronous batch processor that processes input data in batches using
/// a user-defined function and communicates the results back to individual callers.
/// Usefull for processing large amounts of single items data that can be processed more
/// effectively in batches, for example for GPU workloads like ML/AI inference, embeddings,
/// external HTTP API's, rate limited services, image processing, search indexing, etc.
/// </summary>
/// <typeparam name="TIn">The type of the input elements to be processed.</typeparam>
public abstract class AsyncBatchProcessor<TIn> : AsyncBackgroundService
{
	private readonly int _batchSize;
	private readonly ILogger _logger;

	/// <summary>
	/// Represents an unbounded channel used for asynchronous communication
	/// between the producer and consumer within the <see cref="AsyncBatchProcessor{TIn, TOut}"/>.
	/// </summary>
	private readonly Channel<(TIn Value, TaskCompletionSource TaskCompletionSource)> _channel;

	/// <summary>
	/// Preferred constructor: async batch function with cancellation, bounded channel options.
	/// </summary>
	protected AsyncBatchProcessor(
		int batchSize,
		ILogger logger,
		int? capacity = null,
		BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait) : base(logger)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);
		ArgumentNullException.ThrowIfNull(logger);
		if (capacity.HasValue)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(capacity.Value, batchSize);
		}

		_batchSize = batchSize;
		_logger = logger;

		if (capacity.HasValue)
		{
			_channel = Channel.CreateBounded<(TIn, TaskCompletionSource)>(
				new BoundedChannelOptions(capacity.Value)
				{
					SingleReader = true,
					SingleWriter = false,
					FullMode = fullMode
				});
		}
		else
		{
			_channel = Channel.CreateUnbounded<(TIn, TaskCompletionSource)>(
				new UnboundedChannelOptions
				{
					AllowSynchronousContinuations = false,
					SingleReader = true,
					SingleWriter = false
				});
		}
	}

	protected abstract ValueTask ExecuteBatchProcess(ICollection<TIn> inputs, CancellationToken cancellationToken);

	/// Processes a single input asynchronously, enqueues it for batch processing,
	/// and returns a task that completes when the associated output is ready.
	/// <param name="value">
	/// The input value to be processed.
	/// </param>
	/// <param name="cancellationToken">
	/// A cancellation token to observe while waiting for the task to complete. Defaults to none.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous processing operation and contains the resulting output of the task.
	/// </returns>
	protected async Task Process(TIn value,
		CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return;
		}

		// Link the caller's cancellation to the task completion
		TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
		await using CancellationTokenRegistration tokenRegistration =
			cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

		try
		{
			// Preserve completion exceptions by awaiting WriteAsync; will throw ChannelClosedException if closed.
			await _channel.Writer
				.WriteAsync((value, tcs), cancellationToken)
				.ConfigureAwait(false);
		}
		catch (ChannelClosedException)
		{
			// Extract original completion error if available
			Exception? completionError = _channel.Reader.Completion.Exception;
			tcs.TrySetException(completionError ?? new InvalidOperationException("Processor is completed."));
		}
		catch (OperationCanceledException)
		{
			tcs.TrySetCanceled(cancellationToken);
		}
	}

	/// <summary>
	/// Signals that the asynchronous batch processor should complete its operation.
	/// </summary>
	/// <param name="error">
	/// Optional exception to indicate an error condition during the completion process.
	/// If null, the completion is considered successful; otherwise, the specified exception
	/// will propagate as the cause of the completion.
	/// </param>
	public void Complete(Exception? error = null)
	{
		if (error is null)
		{
			_logger.LogInformation("AsyncBatchProcessor completing gracefully.");
			_channel.Writer.TryComplete();
		}
		else
		{
			_logger.LogError(error, "AsyncBatchProcessor completing due to error.");
			_channel.Writer.TryComplete(error);
		}
	}

	/// <summary>
	/// Processes data in batches using the provided function, reading from the input channel
	/// until completion or cancellation.
	/// </summary>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await foreach (ICollection<(TIn Value, TaskCompletionSource TaskCompletionSource)> batch in
			_channel.ReadAllBatch(_batchSize, stoppingToken))
		{
			// If channel completed with an error, fault all pending items without invoking the func
			if (_channel.Reader.Completion.IsFaulted)
			{
				Exception ex = _channel.Reader.Completion.Exception!;
				foreach ((_, TaskCompletionSource tcs) in batch)
				{
					tcs.TrySetException(ex);
				}

				continue;
			}

			// Filter out items canceled by callers to avoid wasted work
			List<(TIn Value, TaskCompletionSource Tcs)> active = new(capacity: batch.Count);
			foreach ((TIn Value, TaskCompletionSource TaskCompletionSource) item in batch)
			{
				if (item.TaskCompletionSource.Task is not { IsCanceled: false, IsCompleted: false }) continue;
				active.Add((item.Value, item.TaskCompletionSource));
			}

			if (active.Count == 0)
			{
				continue;
			}

			try
			{
				TIn[] inputs = active.Select(tuple => tuple.Value).ToArray();
				await ExecuteBatchProcess(inputs, stoppingToken).ConfigureAwait(false);

				for (int i = 0; i < active.Count; i++)
				{
					active[i].Tcs.TrySetResult();
				}
			}
			catch (OperationCanceledException)
			{
				// Cancel remaining not-yet-completed items in the batch with the service stopping token
				foreach ((_, TaskCompletionSource tcs) in active)
				{
					tcs.TrySetCanceled(stoppingToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while processing batch of size {BatchSize}.", active.Count);
				// Propagate the error to all tasks in the active subset
				foreach ((_, TaskCompletionSource tcs) in active)
				{
					tcs.TrySetException(ex);
				}
			}
		}
	}
}
