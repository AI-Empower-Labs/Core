using System.Text.Json;
using System.Text.Json.Serialization;

namespace AEL.Core.Serialization;

public sealed class StringOrIntToIntJsonConverter : JsonConverter<int?>
{
	public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number when reader.TryGetInt32(out int intValue) => intValue,
			JsonTokenType.String => int.TryParse(reader.GetString(), out int intValue) ? intValue : null,
			_ => throw new JsonException($"Unable to convert {reader.TokenType} to string for CustomerCode")
		};
	}

	public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			writer.WriteNumberValue(value.Value);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}