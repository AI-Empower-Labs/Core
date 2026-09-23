using Microsoft.Extensions.Logging;

namespace System.Text.Json;

public static class JsonElementExtensions
{
	extension(JsonElement jsonElement)
	{
		/// <summary>
		/// Deserializes the element, logging and returning <c>default</c> on failure.
		/// </summary>
		public T? SafeDeserialize<T>(ILogger? logger)
		{
			try
			{
				return jsonElement.Deserialize<T>();
			}
			catch (Exception e)
			{
				logger?.LogError(e, "Error while deserializing JSON element: {Json}", jsonElement.ToString());
				return default;
			}
		}
	}

	extension(JsonDocument jsonDocument)
	{
		/// <summary>
		/// Deserializes the document, logging and returning <c>default</c> on failure.
		/// </summary>
		public T? SafeDeserialize<T>(ILogger? logger) => jsonDocument.RootElement.SafeDeserialize<T>(logger);
	}
}
