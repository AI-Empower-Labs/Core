using System.Text;
using System.Text.Json;

using AEL.Core.Json;

using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
	extension(string text)
	{
		public string ToPascalCase()
		{
			if (text.Length == 0 || char.IsLower(text[0]))
			{
				return text;
			}

			return text.Length == 1
				? $"{char.ToLowerInvariant(text[0])}"
				: $"{char.ToLowerInvariant(text[0])}{text[1..]}";
		}

		public T? SafeDeserialize<T>(ILogger? logger = null)
		{
			try
			{
				return JsonSerializer.Deserialize<T>(text);
			}
			catch (Exception firstException)
			{
				string repaired = new JsonRepair().RepairJson(text);
				if (string.IsNullOrEmpty(repaired) || repaired.Equals("[]"))
				{
					return default;
				}

				try
				{
					return JsonSerializer.Deserialize<T>(repaired);
				}
				catch (Exception exception)
				{
					Diagnostics.Debug.WriteLine("Error while deserializing JSON element: {Json}", text);
					logger?.LogError(new AggregateException(firstException, exception), "Error while deserializing JSON element: {Json}", text);
					return default;
				}
			}
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

#pragma warning disable CS0168 // Variable is declared but never used
}
