using System.Collections.Concurrent;

// ReSharper disable once CheckNamespace
namespace System.Linq;

public static class EnumerableExtensions
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : notnull
	{
		return enumerable.OfType<T>();
	}

	/// <summary>
	/// Executes an asynchronous transformation function on each element of the source sequence
	/// in parallel while preserving the original order of results.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
	/// <typeparam name="TResult">The type of the result produced by the transformation function.</typeparam>
	/// <param name="source">The source sequence.</param>
	/// <param name="func">The asynchronous transformation function to apply to each element.</param>
	/// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations.
	/// Use <see langword="null"/> (default) to let <see cref="Parallel.ForEachAsync"/> choose
	/// an appropriate value (ProcessorCount for this overload).</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>An array of results in the same order as the source sequence.</returns>
	public static async Task<TResult[]> SelectAsync<T, TResult>(
		this IEnumerable<T> source,
		Func<T, CancellationToken, Task<TResult>> func,
		int? maxDegreeOfParallelism = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(func);

		// Materialize only when necessary; preserves order and enables direct indexing.
		IReadOnlyList<T> items = source as IReadOnlyList<T> ?? source.ToList();

		if (items.Count == 0)
		{
			return [];
		}

		TResult[] results = new TResult[items.Count];
		ParallelOptions options = new()
		{
			CancellationToken = cancellationToken,
			// Matches the framework default for Parallel.ForEachAsync when null is supplied.
			MaxDegreeOfParallelism = maxDegreeOfParallelism.GetValueOrDefault(-1)
		};

		await Parallel.ForEachAsync(
			Enumerable.Range(0, items.Count),
			options,
			async (index, token) =>
			{
				results[index] = await func(items[index], token).ConfigureAwait(false);
			}).ConfigureAwait(false);

		return results;
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
	}
}
