// ReSharper disable once CheckNamespace
namespace System.Linq;

public static class EnumerableExtensions
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : notnull
	{
		return enumerable.OfType<T>();
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
