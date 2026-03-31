// ReSharper disable once CheckNamespace

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
	}
}
