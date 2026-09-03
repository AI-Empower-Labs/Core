using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using AEL.Core.Serialization;

namespace AEL.Core.Tests.Serialization;

public sealed class JsonConverterTests
{
	private sealed class StringHolder
	{
		[JsonConverter(typeof(StringOrIntToStringJsonConverter))]
		public string? Value { get; set; }
	}

	private sealed class IntHolder
	{
		[JsonConverter(typeof(StringOrIntToIntJsonConverter))]
		public int? Value { get; set; }
	}

	private sealed class UnixDateHolder
	{
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTimeOffset Timestamp { get; set; }
	}

	public enum PriorityLevel
	{
		[EnumMember(Value = "p-low")]
		Low,
		[EnumMember(Value = "p-high")]
		High,
		Normal
	}

	private sealed class EnumHolder
	{
		[JsonConverter(typeof(JsonStringEnumConverterWithEnumMemberAttrSupport<PriorityLevel>))]
		public PriorityLevel Priority { get; set; }
	}

	[Fact]
	public void StringOrIntToString_Deserializes_Int_Int64_Decimal_And_String()
	{
		StringHolder? fromInt = JsonSerializer.Deserialize<StringHolder>("""{"Value": 42}""");
		Assert.Equal("42", fromInt?.Value);

		StringHolder? fromInt64 = JsonSerializer.Deserialize<StringHolder>("""{"Value": 9876543210123}""");
		Assert.Equal("9876543210123", fromInt64?.Value);

		StringHolder? fromString = JsonSerializer.Deserialize<StringHolder>("""{"Value": "hello"}""");
		Assert.Equal("hello", fromString?.Value);

		StringHolder? fromNull = JsonSerializer.Deserialize<StringHolder>("""{"Value": null}""");
		Assert.Null(fromNull?.Value);
	}

	[Fact]
	public void StringOrIntToString_Serializes_Correctly()
	{
		string json = JsonSerializer.Serialize(new StringHolder { Value = "123" });
		Assert.Equal("""{"Value":"123"}""", json);

		string jsonNull = JsonSerializer.Serialize(new StringHolder { Value = null });
		Assert.Equal("""{"Value":null}""", jsonNull);
	}

	[Fact]
	public void StringOrIntToInt_Deserializes_Int_String_And_Invalid()
	{
		IntHolder? fromInt = JsonSerializer.Deserialize<IntHolder>("""{"Value": 42}""");
		Assert.Equal(42, fromInt?.Value);

		IntHolder? fromString = JsonSerializer.Deserialize<IntHolder>("""{"Value": "42"}""");
		Assert.Equal(42, fromString?.Value);

		IntHolder? fromInvalid = JsonSerializer.Deserialize<IntHolder>("""{"Value": "not-a-number"}""");
		Assert.Null(fromInvalid?.Value);

		IntHolder? fromNull = JsonSerializer.Deserialize<IntHolder>("""{"Value": null}""");
		Assert.Null(fromNull?.Value);
	}

	[Fact]
	public void StringOrIntToInt_Serializes_Correctly()
	{
		string json = JsonSerializer.Serialize(new IntHolder { Value = 42 });
		Assert.Equal("""{"Value":42}""", json);

		string jsonNull = JsonSerializer.Serialize(new IntHolder { Value = null });
		Assert.Equal("""{"Value":null}""", jsonNull);
	}

	[Fact]
	public void UnixDateTimeConverter_RoundTrips_Seconds()
	{
		DateTimeOffset now = DateTimeOffset.FromUnixTimeSeconds(1700000000);
		string json = JsonSerializer.Serialize(new UnixDateHolder { Timestamp = now });
		Assert.Equal("""{"Timestamp":1700000000}""", json);

		UnixDateHolder? deserialized = JsonSerializer.Deserialize<UnixDateHolder>(json);
		Assert.Equal(now, deserialized?.Timestamp);
	}

	[Fact]
	public void EnumWithEnumMember_SerializesAndDeserializes()
	{
		EnumHolder high = new() { Priority = PriorityLevel.High };
		string jsonHigh = JsonSerializer.Serialize(high);
		Assert.Equal("""{"Priority":"p-high"}""", jsonHigh);

		EnumHolder? desHigh = JsonSerializer.Deserialize<EnumHolder>("""{"Priority":"p-high"}""");
		Assert.Equal(PriorityLevel.High, desHigh?.Priority);

		EnumHolder normal = new() { Priority = PriorityLevel.Normal };
		string jsonNormal = JsonSerializer.Serialize(normal);
		Assert.Equal("""{"Priority":"Normal"}""", jsonNormal);

		EnumHolder? desNormal = JsonSerializer.Deserialize<EnumHolder>("""{"Priority":"Normal"}""");
		Assert.Equal(PriorityLevel.Normal, desNormal?.Priority);
	}
}
