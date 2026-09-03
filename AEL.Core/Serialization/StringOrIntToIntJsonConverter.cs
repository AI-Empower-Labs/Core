using System.Globalization;
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
			JsonTokenType.String => int.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue) ? intValue : null,
			JsonTokenType.Null => null,
			_ => throw new JsonException($"Unable to convert {reader.TokenType} to integer.")
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
