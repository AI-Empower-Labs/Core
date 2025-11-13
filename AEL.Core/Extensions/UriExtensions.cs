// ReSharper disable once CheckNamespace

namespace System.Linq;

#if NET10_0_OR_GREATER
public static class UriExtensions
{
	/// <param name="uri">The source URI.</param>
	extension(Uri uri)
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

		/// <summary>
		/// Appends query parameters to the URI using a query-like operator.
		/// </summary>
		/// <remarks>
		/// C# does not allow overloading the '?' operator. As a close analogue, this overload uses '&' to
		/// append query text. The <paramref name="right"/> value can be either a full query string (e.g. "a=1&b=2")
		/// or start with '?' (e.g. "?a=1"). If a fragment ("#...") is present, it will be applied to the result.
		/// Values are not re-encoded; pass already-escaped text if needed.
		/// </remarks>
		/// <param name="left">Base <see cref="Uri"/>.</param>
		/// <param name="right">Query text to append. May optionally start with '?' and/or contain a fragment.</param>
		/// <returns>A new <see cref="Uri"/> with the appended query and optional fragment.</returns>
		public static Uri operator &(Uri left, string right)
		{
			ArgumentNullException.ThrowIfNull(left);
			if (string.IsNullOrEmpty(right))
			{
				return left;
			}

			string append = right[0] == '?' ? right[1..] : right;
			if (append.Length == 0)
			{
				return left;
			}

			int hashIdx = append.IndexOf('#');
			string queryPart = hashIdx >= 0 ? append[..hashIdx] : append;
			string? fragmentPart = hashIdx >= 0 ? append[(hashIdx + 1)..] : null;

			UriBuilder builder = new(left);
			string existing = builder.Query;
			if (!string.IsNullOrEmpty(existing) && existing[0] == '?')
			{
				existing = existing[1..];
			}

			builder.Query = string.IsNullOrEmpty(existing)
				? queryPart
				: string.IsNullOrEmpty(queryPart)
					? existing
					: $"{existing}&{queryPart}";

			if (fragmentPart is not null)
			{
				builder.Fragment = fragmentPart;
			}

			return builder.Uri;
		}
	}
}
#endif
