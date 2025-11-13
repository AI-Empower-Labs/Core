using System.Collections.Concurrent;

// ReSharper disable once CheckNamespace
namespace System.Linq;

public static class EnumerableExtensions
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : notnull
	{
		return enumerable.OfType<T>();
	}

	public static async Task<TResult[]> ForEachParallel<T, TResult>(this IEnumerable<T> source,
		Func<T, CancellationToken, Task<TResult>> func,
		int? maxDegreeOfParallelism = null,
		CancellationToken cancellationToken = default)
	{
		ConcurrentQueue<(int, TResult)> results = [];
		await Parallel
			.ForEachAsync(source.Index(),
				new ParallelOptions
				{
					CancellationToken = cancellationToken,
					MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Math.Min(1, Environment.ProcessorCount / 4)
				},
				async (item, token) =>
				{
					TResult result = await func(item.Item, token);
					results.Enqueue((item.Index, result));
				});
		return results
			.OrderBy(tuple => tuple.Item1)
			.Select(tuple => tuple.Item2)
			.ToArray();
	}

#if NET10_0_OR_GREATER
	extension(IEnumerable<string> source)
	{
		/// <summary>
		/// Joins the strings in the sequence with the specified separator.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string operator *(IEnumerable<string> left, string separator)
			=> string.Join(separator, left);
	}
#endif
}
