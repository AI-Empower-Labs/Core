using System.Text.Json;
using System.Text.Json.Serialization;

namespace AEL.Core.Serialization;

public sealed class StringOrIntToStringJsonConverter : JsonConverter<string>
{
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number when reader.TryGetInt32(out int intValue) => intValue.ToString(),
			JsonTokenType.String => reader.GetString(),
			_ => throw new JsonException($"Unable to convert {reader.TokenType} to string for CustomerCode")
		};
	}

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value);
	}
}
