using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AEL.Core.Serialization;

public sealed class StringOrIntToStringJsonConverter : JsonConverter<string>
{
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number when reader.TryGetInt64(out long longValue) => longValue.ToString(CultureInfo.InvariantCulture),
			JsonTokenType.Number when reader.TryGetDouble(out double doubleValue) => doubleValue.ToString(CultureInfo.InvariantCulture),
			JsonTokenType.Number => Encoding.UTF8.GetString(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan),
			JsonTokenType.String => reader.GetString(),
			JsonTokenType.Null => null,
			_ => throw new JsonException($"Unable to convert {reader.TokenType} to string.")
		};
	}

	public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
		}
		else
		{
			writer.WriteStringValue(value);
		}
	}
}
