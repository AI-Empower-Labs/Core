using System.Threading.Channels;

using AEL.Core.Extensions;
// ReSharper disable once CheckNamespace
using System.Runtime.CompilerServices;

namespace System.Linq;

public static class AsyncEnumerableExtensions
{
	extension<T>(IAsyncEnumerable<T?> enumerable) where T : notnull
	{
		public async IAsyncEnumerable<T> WhereNotNull()
		{
			await foreach (T? t in enumerable)
			{
				if (t is not null)
				{
					yield return t;
				}
			}
		}
	}

	extension<T>(IAsyncEnumerable<T> enumerable)
	{
		public async IAsyncEnumerable<ICollection<T>> Batch(int batchSize,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			Channel<T> channel = Channel.CreateBounded<T>(batchSize);
			Task producerTask = Producer();
			await foreach (ICollection<T> collection in channel.ReadAllBatch(batchSize, cancellationToken))
			{
				yield return collection;
			}

			await producerTask;

			async Task Producer()
			{
				try
				{
					await foreach (T t in enumerable.WithCancellation(cancellationToken))
					{
						await channel.Writer.WriteAsync(t, cancellationToken);
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					// Ignore
				}
				catch (Exception ex)
				{
					channel.Writer.TryComplete(ex);
				}
				finally
				{
					channel.Writer.TryComplete();
				}
			}
		}

		public async IAsyncEnumerable<ICollection<T>> BatchWithDrain(int batchSize,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			Channel<T> channel = Channel.CreateBounded<T>(batchSize);
			Task producerTask = Producer();
			await foreach (ICollection<T> collection in channel.ReadAllBatchDrain(batchSize, cancellationToken))
			{
				yield return collection;
			}

			await producerTask;

			async Task Producer()
			{
				try
				{
					await foreach (T t in enumerable.WithCancellation(cancellationToken))
					{
						await channel.Writer.WriteAsync(t, cancellationToken);
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					// Ignore
				}
				catch (Exception ex)
				{
					channel.Writer.TryComplete(ex);
				}
				finally
				{
					channel.Writer.TryComplete();
				}
			}
		}

		public async IAsyncEnumerable<TResult> ForEachParallel<TResult>(
			Func<T, CancellationToken, Task<TResult>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			Queue<Task<TResult>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);

			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			await foreach (T item in enumerable.WithCancellation(cancellationToken))
			{
				await semaphore.WaitAsync(cancellationToken);

				// Define the task
				Task<TResult> task = Task.Run(async () =>
				{
					try { return await selector(item, tokenSource.Token); }
					finally { semaphore.Release(); }
				}, cancellationToken);

				queue.Enqueue(task);

				// While the oldest task in the queue is finished, yield it
				while (queue.Count > 0 && queue.Peek().IsCompleted)
				{
					yield return await queue.Dequeue();
				}
			}

			// Yield remaining tasks
			while (queue.Count > 0)
			{
				yield return await queue.Dequeue();
			}
		}
	}
}
