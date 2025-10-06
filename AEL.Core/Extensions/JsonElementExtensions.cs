using Microsoft.Extensions.Logging;

#pragma warning disable CS0168 // Variable is declared but never used

namespace System.Text.Json;

public static class JsonElementExtensions
{
	public static T? SafeDeserialize<T>(this JsonElement jsonElement, ILogger? logger)
	{
		try
		{
			return jsonElement.Deserialize<T>();
		}
		catch (Exception e)
		{
#if DEBUG
			Debug.WriteLine("Error while deserializing JSON element: {Json}", jsonElement.ToString());
#else
			logger?.LogError(e, "Error while deserializing JSON element: {Json}", jsonElement);
#endif
			return default;
		}
	}

	public static T? SafeDeserialize<T>(this JsonDocument jsonDocument, ILogger? logger)
	{
		try
		{
			return jsonDocument.Deserialize<T>();
		}
		catch (Exception e)
		{
#if DEBUG
			Debug.WriteLine("Error while deserializing JSON element: {Json}", jsonDocument.RootElement.ToString());
#else
			logger?.LogError(e, "Error while deserializing JSON element: {Json}", jsonDocument.RootElement);
#endif
			return default;
		}
	}
}
