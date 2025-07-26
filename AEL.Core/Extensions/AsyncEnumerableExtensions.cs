using System.Threading.Channels;

using AEL.Core.Extensions;
// ReSharper disable once CheckNamespace
using System.Runtime.CompilerServices;

namespace System.Linq;

public static class AsyncEnumerableExtensions
{
	public static async IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T?> enumerable) where T : notnull
	{
		await foreach (T? t in enumerable)
		{
			if (t is not null)
			{
				yield return t;
			}
		}
	}

	public static async IAsyncEnumerable<ICollection<T>> Batch<T>(this IAsyncEnumerable<T> enumerable,
		int batchSize,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Channel<T> channel = Channel.CreateBounded<T>(batchSize);
		Task producerTask = Producer();
		await foreach (ICollection<T> collection in channel.ReadAllBatchAggressive(batchSize, cancellationToken))
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
			finally
			{
				channel.Writer.Complete();
			}
		}
	}

	public static async IAsyncEnumerable<TResult> ForeachParallel<T, TResult>(
		this IEnumerable<T> source,
		Func<T, CancellationToken, Task<TResult>> func,
		int? maxDegreeOfParallelism = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		Channel<(int Index, TResult Result)> channel = Channel.CreateUnbounded<(int Index, TResult Result)>(
			new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
		Task producer = Task.Run(Producer, cancellationToken);
		SortedDictionary<int, TResult> resultDict = new();
		int yielded = 0;
		await foreach ((int index, TResult result) in channel.Reader.ReadAllAsync(cancellationToken))
		{
			resultDict[index] = result;
			while (resultDict.TryGetValue(yielded, out TResult? nextResult))
			{
				yield return nextResult;
				resultDict.Remove(yielded);
				yielded++;
			}
		}

		await producer;
		yield break;

		// Producer task pushes results into the channel as they are completed
		async Task? Producer()
		{
			int degree = maxDegreeOfParallelism ?? Math.Max(1, Environment.ProcessorCount / 4);
			SemaphoreSlim throttler = new(degree);
			int idx = 0;
			List<Task> tasks = [];
			try
			{
				foreach (T item in source)
				{
					await throttler.WaitAsync(cancellationToken);
					int currentIndex = idx++;
					Task task = Task.Run(async () =>
					{
						try
						{
							TResult result = await func(item, cancellationToken);
							await channel.Writer.WriteAsync((currentIndex, result), cancellationToken);
						}
						finally
						{
							throttler.Release();
						}
					}, cancellationToken);
					tasks.Add(task);
				}

				await Task.WhenAll(tasks);
			}
			finally
			{
				channel.Writer.Complete();
			}
		}
	}
}
