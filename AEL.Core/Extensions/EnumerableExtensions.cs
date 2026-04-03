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
		/// <param name="left"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string operator *(IEnumerable<T> left, string separator) => string.Join(separator, left);

		public async IAsyncEnumerable<TResult> ForEachParallel<TResult>(
			Func<T, CancellationToken, Task<TResult>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : notnull
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

			Queue<Task<TResult>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			foreach (T item in source)
			{
				await semaphore.WaitAsync(tokenSource.Token).ConfigureAwait(false);
				queue.Enqueue(RunSelector(item, tokenSource.Token, semaphore));
				while (queue.Count > 0 && queue.Peek().IsCompleted)
				{
					yield return await DequeueHandled(tokenSource).ConfigureAwait(false);
				}
			}

			while (queue.Count > 0)
			{
				yield return await DequeueHandled(tokenSource).ConfigureAwait(false);
			}

			yield break;

			async Task<TResult> RunSelector(T item, CancellationToken token, SemaphoreSlim concurrencyLimiter)
			{
				try
				{
					return await selector(item, token).ConfigureAwait(false);
				}
				finally
				{
					concurrencyLimiter.Release();
				}
			}

			async Task<TResult> DequeueHandled(CancellationTokenSource cancellationTokenSource)
			{
				try
				{
					return await queue.Dequeue().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					await cancellationTokenSource.CancelAsync();

					// Observe remaining tasks to prevent UnobservedTaskException
					List<Exception> exceptions = [];
					foreach (Task<TResult> remaining in queue)
					{
						try { await remaining.ConfigureAwait(false); }
						catch (Exception ee) { exceptions.Add(ee); }
					}

					throw new AggregateException(exceptions.Prepend(e));
				}
			}
		}

		public async IAsyncEnumerable<TResult?> ForEachParallelNullable<TResult>(
			Func<T, CancellationToken, Task<TResult?>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : notnull
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

			Queue<Task<TResult?>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);
			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			foreach (T item in source)
			{
				await semaphore.WaitAsync(tokenSource.Token).ConfigureAwait(false);
				queue.Enqueue(RunSelector(item, tokenSource.Token, semaphore));
				while (queue.Count > 0 && queue.Peek().IsCompleted)
				{
					yield return await DequeueHandled(tokenSource).ConfigureAwait(false);
				}
			}

			while (queue.Count > 0)
			{
				yield return await DequeueHandled(tokenSource).ConfigureAwait(false);
			}

			yield break;

			async Task<TResult?> RunSelector(T item, CancellationToken token, SemaphoreSlim concurrencyLimiter)
			{
				try
				{
					return await selector(item, token).ConfigureAwait(false);
				}
				finally
				{
					concurrencyLimiter.Release();
				}
			}

			async Task<TResult?> DequeueHandled(CancellationTokenSource cancellationTokenSource)
			{
				try
				{
					return await queue.Dequeue().ConfigureAwait(false);
				}
				catch (Exception e)
				{
					await cancellationTokenSource.CancelAsync();

					// Observe remaining tasks to prevent UnobservedTaskException
					List<Exception> exceptions = [];
					foreach (Task<TResult?> remaining in queue)
					{
						try { await remaining.ConfigureAwait(false); }
						catch (Exception ee) { exceptions.Add(ee); }
					}

					throw new AggregateException(exceptions.Prepend(e));
				}
			}
		}
	}
}
