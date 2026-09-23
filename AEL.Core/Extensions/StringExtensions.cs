using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
	extension(string text)
	{
		public string ToPascalCase()
		{
			if (text.Length == 0)
			{
				return text;
			}

			return text.Length == 1
				? char.ToUpperInvariant(text[0]).ToString()
				: $"{char.ToUpperInvariant(text[0])}{text[1..]}";
		}
	}

	extension(string? text)
	{
		/// <summary>
		/// Removes control characters, trims each line and drops empty lines. Lines are joined with <c>\n</c>.
		/// </summary>
		public string Sanitize()
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			StringBuilder builder = new();
			foreach (string line in text.Split(['\n', '\r'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
			{
				int lineStart = builder.Length;
				foreach (char c in line.Where(c => !char.IsControl(c)))
				{
					builder.Append(c);
				}

				if (builder.Length > lineStart)
				{
					builder.Append('\n');
				}
			}

			return builder.ToString().TrimEnd('\n');
		}
	}
}
