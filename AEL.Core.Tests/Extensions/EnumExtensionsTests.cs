using System.Runtime.Serialization;

using AEL.Core.Extensions;

namespace AEL.Core.Tests.Extensions;

public class EnumExtensionsTests
{
	public enum Color
	{
		[EnumMember(Value = "r")] Red,
		[EnumMember(Value = "g")] Green,
		Blue
	}

	[Theory]
	[InlineData("Red", true, Color.Red)]
	[InlineData("r", true, Color.Red)]
	[InlineData("GREEN", true, Color.Green)]
	[InlineData("g", true, Color.Green)]
	[InlineData("unknown", false, Color.Red)]
	public void TryParseEnumValue_ParsesByNameOrEnumMember(string input, bool expected, Color expectedValue)
	{
		bool ok = input.TryParseEnumValue(out Color result);
		Assert.Equal(expected, ok);
		if (ok) Assert.Equal(expectedValue, result);
	}

	[Fact]
	public void ParseEnumValue_ReturnsDefaultOnNullOrInvalid()
	{
		Assert.Equal(Color.Blue, ((string?)null).ParseEnumValue(Color.Blue));
		Assert.Equal(Color.Blue, "bad".ParseEnumValue(Color.Blue));
		Assert.Equal(Color.Red, "r".ParseEnumValue(Color.Blue));
	}

	[Fact]
	public void GetEnumNames_ReturnsEnumMemberValues()
	{
		string[] names = EnumExtensions.GetEnumNames<Color>();
		Assert.Contains("r", names);
		Assert.Contains("g", names);
		Assert.Contains("Blue", names);

		string[] allowed = EnumExtensions.GetEnumNames([Color.Red, Color.Blue]);
		Assert.Equal(new[] { "r", "Blue" }, allowed);
	}

	[Fact]
	public void GetEnumName_ReturnsCorrectName()
	{
		Assert.Equal("r", Color.Red.GetEnumName());
		Assert.Equal("Blue", Color.Blue.GetEnumName());
	}
}
