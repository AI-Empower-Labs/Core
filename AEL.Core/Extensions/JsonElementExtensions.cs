using Microsoft.Extensions.Logging;

#pragma warning disable CS0168 // Variable is declared but never used

namespace System.Text.Json;

public static class JsonElementExtensions
{
	extension(JsonElement jsonElement)
	{
		public T? SafeDeserialize<T>(ILogger? logger)
		{
			try
			{
				return jsonElement.Deserialize<T>();
			}
			catch (Exception e)
			{
#if DEBUG
				Diagnostics.Debug.WriteLine("Error while deserializing JSON element: {Json}", jsonElement.ToString());
#else
			logger?.LogError(e, "Error while deserializing JSON element: {Json}", jsonElement);
#endif
				return default;
			}
		}
	}

	extension(JsonDocument jsonDocument)
	{
		public T? SafeDeserialize<T>(ILogger? logger)
		{
			try
			{
				return jsonDocument.Deserialize<T>();
			}
			catch (Exception e)
			{
#if DEBUG
				Diagnostics.Debug.WriteLine("Error while deserializing JSON element: {Json}", jsonDocument.RootElement.ToString());
#else
			logger?.LogError(e, "Error while deserializing JSON element: {Json}", jsonDocument.RootElement);
#endif
				return default;
			}
		}
	}
}
