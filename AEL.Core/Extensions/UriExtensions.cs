// ReSharper disable once CheckNamespace

namespace System.Linq;

public static class UriExtensions
{
#if NET10_0_OR_GREATER
	extension(Uri source)
	{
		/// <summary>
		/// Joins the two URIs together.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Uri operator /(Uri left, Uri right) => new(left, right);

		/// <summary>
		/// Joins the URI with the specified relative path.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Uri operator /(Uri left, string right) => new(left, right);
	}
#endif
}
