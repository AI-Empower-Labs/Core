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

	/// <param name="text">The text to sanitize. Can be null.</param>
	extension(string? text)
	{
		/// <summary>
		/// Sanitizes the input text by removing control characters and normalizing line breaks.
		/// </summary>
		/// <returns>A sanitized string with control characters removed and consistent line breaks, or empty string if input is null/empty.</returns>
		public string Sanitize()
		{
			// Return empty string if input is null or empty
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			// Use StringBuilder for efficient string concatenation
			StringBuilder builder = new();

			// Split text into lines, trimming whitespace and removing empty lines
			string[] lines = text.Split(['\n', '\r'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

			// Process each line
			foreach (string line in lines)
			{
				// Filter out control characters (like tabs, carriage returns, etc.) and append valid characters
				foreach (char c in line.Where(c => !char.IsControl(c)))
				{
					builder.Append(c);
				}

				// Only append '\n' if we actually added characters for this line,
				// and avoid more than one blank line (max two consecutive '\n')
				if ((builder.Length < 2 || builder[^1] != '\n' || builder[^2] != '\n'))
				{
					builder.Append('\n');
				}
			}

			return builder.ToString().Trim('\n');
		}
	}

#pragma warning disable CS0168 // Variable is declared but never used
}
