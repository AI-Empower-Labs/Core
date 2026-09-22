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
			ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

			Channel<T> channel = Channel.CreateBounded<T>(batchSize);
			using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			Task producerTask = Producer(linkedCts.Token);

			await using (Disposables.DeferAsync((linkedCts, channel, producerTask), static async (state, token) =>
				{
					await state.linkedCts.CancelAsync();
					state.channel.Writer.TryComplete();
					await state.producerTask.WithSilentCancellation(cancellationToken: token);
				}))
			{
				await foreach (ICollection<T> collection in channel.ReadAllBatch(batchSize, linkedCts.Token))
				{
					yield return collection;
				}
			}

			yield break;

			async Task Producer(CancellationToken token)
			{
				try
				{
					await foreach (T t in enumerable.WithCancellation(token))
					{
						await channel.Writer.WriteAsync(t, token);
					}
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					// Ignore
				}
				catch (Exception ex)
				{
					channel.Writer.TryComplete(ex);
					return;
				}

				channel.Writer.TryComplete();
			}
		}

		public async IAsyncEnumerable<ICollection<T>> BatchWithDrain(int batchSize,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

			Channel<T> channel = Channel.CreateBounded<T>(batchSize);
			using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			Task producerTask = Producer(linkedCts.Token);

			await using (Disposables.DeferAsync((linkedCts, channel, producerTask), static async (state, token) =>
				{
					await state.linkedCts.CancelAsync();
					state.channel.Writer.TryComplete();
					await state.producerTask.WithSilentCancellation(cancellationToken: token);
				}))
			{
				await foreach (ICollection<T> collection in channel.ReadAllBatchDrain(batchSize, linkedCts.Token))
				{
					yield return collection;
				}
			}

			yield break;

			async Task Producer(CancellationToken token)
			{
				try
				{
					await foreach (T t in enumerable.WithCancellation(token))
					{
						await channel.Writer.WriteAsync(t, token);
					}
				}
				catch (OperationCanceledException) when (token.IsCancellationRequested)
				{
					// Ignore
				}
				catch (Exception ex)
				{
					channel.Writer.TryComplete(ex);
					return;
				}

				channel.Writer.TryComplete();
			}
		}

		public async IAsyncEnumerable<TResult> ForEachParallel<TResult>(
			Func<T, CancellationToken, Task<TResult>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);
			ArgumentNullException.ThrowIfNull(selector);

			Queue<Task<TResult>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			await using (Disposables.DeferAsync((tokenSource, queue), static async (state, token) =>
				{
					await state.tokenSource.CancelAsync();
					while (state.queue.Count > 0)
					{
						Task<TResult> task = state.queue.Dequeue();
						await task.WithSilentException(cancellationToken: token);
					}
				}))
			{
				await foreach (T item in enumerable.WithCancellation(tokenSource.Token))
				{
					await semaphore.WaitAsync(tokenSource.Token);

					queue.Enqueue(RunSelector(item, tokenSource.Token, semaphore));

					while (queue.Count > 0 && queue.Peek().IsCompleted)
					{
						yield return await DequeueHandled(queue, tokenSource);
					}
				}

				while (queue.Count > 0)
				{
					yield return await DequeueHandled(queue, tokenSource);
				}
			}

			yield break;

			async Task<TResult> RunSelector(T item, CancellationToken token, SemaphoreSlim concurrencyLimiter)
			{
				await using Defer _ = Disposables.Defer(concurrencyLimiter, static limiter =>
				{
					try
					{
						limiter.Release();
					}
					catch (ObjectDisposedException)
					{
						// Ignore in case of teardown
					}
				});

				return await selector(item, token);
			}

			static async Task<TResult> DequeueHandled(Queue<Task<TResult>> q, CancellationTokenSource cts)
			{
				Task<TResult> task = q.Dequeue();
				try
				{
					return await task;
				}
				catch (OperationCanceledException) when (cts.IsCancellationRequested)
				{
					throw;
				}
				catch (Exception e)
				{
					await cts.CancelAsync();

					List<Exception> exceptions = [];
					while (q.Count > 0)
					{
						Task<TResult> remaining = q.Dequeue();
						try
						{
							await remaining;
						}
						catch (OperationCanceledException)
						{
							// Ignore cancellation of other tasks during abort
						}
						catch (Exception ee)
						{
							exceptions.Add(ee);
						}
					}

					if (e is OperationCanceledException && exceptions.Count == 0)
					{
						throw;
					}

					throw exceptions.Count > 0
						? new AggregateException(exceptions.Prepend(e))
						: e;
				}
			}
		}
	}
}
