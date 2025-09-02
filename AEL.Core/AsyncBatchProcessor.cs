using System.Threading.Channels;

using AEL.Core.Extensions;

namespace AEL.Core;

public sealed class AsyncBatchProcessor<TIn, TOut>(
	int batchSize,
	Func<TIn[], TOut[]> func)
{
	private readonly Channel<(TIn Value, TaskCompletionSource<TOut> TaskCompletionSource)> _channel =
		Channel.CreateUnbounded<(TIn, TaskCompletionSource<TOut>)>();

	public async ValueTask<TOut> Process(TIn value,
		CancellationToken cancellationToken = default)
	{
		// Link the caller's cancellation to the task completion
		TaskCompletionSource<TOut> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
		await using CancellationTokenRegistration tokenRegistration =
			cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

		// If cancellation is already requested, avoid enqueueing
		if (cancellationToken.IsCancellationRequested)
		{
			return await tcs.Task.ConfigureAwait(false);
		}

		if (!_channel.Writer.TryWrite((value, tcs)))
		{
			tcs.TrySetException(new InvalidOperationException("Failed to enqueue request."));
		}

		return await tcs.Task;
	}

	public void Complete(Exception? error = null)
	{
		if (error is null)
		{
			_channel.Writer.TryComplete();
		}
		else
		{
			_channel.Writer.TryComplete(error);
		}
	}

	public async Task Execute(CancellationToken cancellationToken)
	{
		await foreach (ICollection<(TIn Value, TaskCompletionSource<TOut> TaskCompletionSource)> batch in
			_channel.ReadAllBatchAggressive(batchSize, cancellationToken))
		{
			try
			{
				TOut[] embeddingOutputs = func(batch.Select(tuple => tuple.Value).ToArray());

				int count = Math.Min(embeddingOutputs.Length, batch.Count);
				int i = 0;
				foreach ((TIn Value, TaskCompletionSource<TOut> TaskCompletionSource) tuple in batch)
				{
					if (i < count)
					{
						tuple.TaskCompletionSource.TrySetResult(embeddingOutputs[i]);
					}
					else
					{
						tuple.TaskCompletionSource.TrySetException(
							new InvalidOperationException("Batch output count mismatch."));
					}

					i++;
				}
			}
			catch (OperationCanceledException oce)
			{
				foreach ((_, TaskCompletionSource<TOut> tcs) in batch)
				{
					tcs.TrySetCanceled(oce.CancellationToken);
				}
			}
			catch (Exception ex)
			{
				// Propagate the error to all tasks in the batch
				foreach ((_, TaskCompletionSource<TOut> tcs) in batch)
				{
					tcs.TrySetException(ex);
				}
			}
		}
	}
}
