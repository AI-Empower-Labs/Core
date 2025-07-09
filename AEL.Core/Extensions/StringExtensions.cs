using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
	public static string ToPascalCase(this string source)
	{
		if (source.Length == 0 || char.IsLower(source[0]))
		{
			return source;
		}

		return source.Length == 1
			? $"{char.ToLowerInvariant(source[0])}"
			: $"{char.ToLowerInvariant(source[0])}{source[1..]}";
	}

	/// <summary>
	/// Sanitizes the input text by removing control characters and normalizing line breaks.
	/// </summary>
	/// <param name="text">The text to sanitize. Can be null.</param>
	/// <returns>A sanitized string with control characters removed and consistent line breaks, or empty string if input is null/empty.</returns>
	public static string Sanitize(this string? text)
	{
		// Return empty string if input is null or empty
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}

		// Use StringBuilder for efficient string concatenation
		StringBuilder builder = new();

		// Split text into lines, trimming whitespace and removing empty lines
		string[] lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

		// Process each line
		foreach (string line in lines)
		{
			// Filter out control characters (like tabs, carriage returns, etc.) and append valid characters
			foreach (char c in line.Where(c => !char.IsControl(c)))
			{
				builder.Append(c);
			}

			// Add a consistent line break after each processed line
			builder.Append('\n');
		}

		return builder.ToString();
	}
}
