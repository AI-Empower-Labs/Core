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
		public static string operator *(IEnumerable<T> left, string separator)
			=> string.Join(separator, left);

		public async IAsyncEnumerable<TResult> ForEachParallel<TResult>(
			Func<T, CancellationToken, Task<TResult>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : notnull
		{
			Queue<Task<TResult>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);

			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			foreach (T item in source)
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

		public async IAsyncEnumerable<TResult?> ForEachParallelNullable<TResult>(
			Func<T, CancellationToken, Task<TResult?>> selector,
			int maxDegreeOfParallelism = 4,
			[EnumeratorCancellation] CancellationToken cancellationToken = default) where TResult : notnull
		{
			Queue<Task<TResult?>> queue = new();
			using SemaphoreSlim semaphore = new(maxDegreeOfParallelism);

			using CancellationTokenSource tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			foreach (T item in source)
			{
				await semaphore.WaitAsync(cancellationToken);

				// Define the task
				Task<TResult?> task = Task.Run(async () =>
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
