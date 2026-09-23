// ReSharper disable once CheckNamespace

using System.Runtime.CompilerServices;

namespace System.Linq;

public static class EnumerableExtensions
{
	extension<T>(IEnumerable<T?> enumerable) where T : notnull
	{
		public IEnumerable<T> WhereNotNull()
		{
			return enumerable.OfType<T>();
		}
	}

	extension<T>(IEnumerable<T> source)
	{
		/// <summary>
		/// Joins the elements in the sequence with the specified separator.
		/// </summary>
		public static string operator *(IEnumerable<T> left, string separator) => string.Join(separator, left);

		public async IAsyncEnumerable<TResult> ForEachParallel<TResult>(
			Func<T, CancellationToken, Task<TResult>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : notnull
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);
			ArgumentNullException.ThrowIfNull(selector);

			Queue<Task<TResult>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			await using (AsyncDefer.Action((tokenSource, queue), static async (state, token) =>
				{
					await state.tokenSource.CancelAsync();
					while (state.queue.Count > 0)
					{
						Task<TResult> remaining = state.queue.Dequeue();
						await remaining.WithSilentException(cancellationToken: token).ConfigureAwait(false);
					}
				}))
			{
				foreach (T item in source)
				{
					await semaphore.WaitAsync(tokenSource.Token).ConfigureAwait(false);
					queue.Enqueue(RunSelector(item, tokenSource.Token, semaphore));
					while (queue.Count > 0 && queue.Peek().IsCompleted)
					{
						yield return await DequeueHandled(queue, tokenSource).ConfigureAwait(false);
					}
				}

				while (queue.Count > 0)
				{
					yield return await DequeueHandled(queue, tokenSource).ConfigureAwait(false);
				}
			}

			yield break;

			async Task<TResult> RunSelector(T item, CancellationToken token, SemaphoreSlim concurrencyLimiter)
			{
				await using Defer _ = Defer.Action(concurrencyLimiter, static limiter =>
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

				return await selector(item, token).ConfigureAwait(false);
			}

			static async Task<TResult> DequeueHandled(Queue<Task<TResult>> q, CancellationTokenSource cancellationTokenSource)
			{
				Task<TResult> task = q.Dequeue();
				try
				{
					return await task.ConfigureAwait(false);
				}
				catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
				{
					throw;
				}
				catch (Exception e)
				{
					await cancellationTokenSource.CancelAsync();

					// Observe remaining tasks to prevent UnobservedTaskException
					List<Exception> exceptions = [];
					while (q.Count > 0)
					{
						Task<TResult> remaining = q.Dequeue();
						try
						{
							await remaining.ConfigureAwait(false);
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

		public async IAsyncEnumerable<TResult?> ForEachParallelNullable<TResult>(
			Func<T, CancellationToken, Task<TResult?>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : notnull
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);
			ArgumentNullException.ThrowIfNull(selector);

			Queue<Task<TResult?>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			await using (AsyncDefer.Action((tokenSource, queue), static async (state, token) =>
				{
					await state.tokenSource.CancelAsync();
					while (state.queue.Count > 0)
					{
						Task<TResult?> remaining = state.queue.Dequeue();
						await remaining.WithSilentException(cancellationToken: token).ConfigureAwait(false);
					}
				}))
			{
				foreach (T item in source)
				{
					await semaphore.WaitAsync(tokenSource.Token).ConfigureAwait(false);
					queue.Enqueue(RunSelector(item, tokenSource.Token, semaphore));
					while (queue.Count > 0 && queue.Peek().IsCompleted)
					{
						yield return await DequeueHandled(queue, tokenSource).ConfigureAwait(false);
					}
				}

				while (queue.Count > 0)
				{
					yield return await DequeueHandled(queue, tokenSource).ConfigureAwait(false);
				}
			}

			yield break;

			async Task<TResult?> RunSelector(T item, CancellationToken token, SemaphoreSlim concurrencyLimiter)
			{
				await using Defer _ = Defer.Action(concurrencyLimiter, static limiter =>
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

				return await selector(item, token).ConfigureAwait(false);
			}

			static async Task<TResult?> DequeueHandled(Queue<Task<TResult?>> q, CancellationTokenSource cancellationTokenSource)
			{
				Task<TResult?> task = q.Dequeue();
				try
				{
					return await task.ConfigureAwait(false);
				}
				catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
				{
					throw;
				}
				catch (Exception e)
				{
					await cancellationTokenSource.CancelAsync();

					// Observe remaining tasks to prevent UnobservedTaskException
					List<Exception> exceptions = [];
					while (q.Count > 0)
					{
						Task<TResult?> remaining = q.Dequeue();
						try
						{
							await remaining.ConfigureAwait(false);
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
