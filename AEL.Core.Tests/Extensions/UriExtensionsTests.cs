namespace AEL.Core.Tests.Extensions;

public sealed class UriExtensionsTests
{
	[Fact]
	public void SlashOperator_Joins_UriAndString_WithTrailingSlashOnBase()
	{
		Uri baseUri = new("https://example.com/api/");
		Uri result = baseUri / "v1/items";

		Assert.Equal(new Uri("https://example.com/api/v1/items"), result);
	}

	[Fact]
	public void SlashOperator_Joins_UriAndString_BaseWithoutTrailingSlash_ReplacesLastSegment()
	{
		// When base URI lacks trailing slash, System.Uri treats last segment as a file and replaces it
		Uri baseUri = new("https://example.com/api");
		Uri result = baseUri / "v1";

		Assert.Equal(new Uri("https://example.com/v1"), result);
	}

	[Fact]
	public void SlashOperator_Joins_UriAndString_RightWithLeadingSlash_GoesToRoot()
	{
		Uri baseUri = new("https://example.com/api/");
		Uri result = baseUri / "/v1";

		Assert.Equal(new Uri("https://example.com/v1"), result);
	}

	[Fact]
	public void SlashOperator_UriAndString_RightIsAbsolute_OverridesBase()
	{
		Uri baseUri = new("https://example.com/api/");
		Uri result = baseUri / "https://other.com/x";

		Assert.Equal(new Uri("https://other.com/x"), result);
	}

	[Fact]
	public void SlashOperator_UriAndUri_RightRelative_Joins()
	{
		Uri baseUri = new("https://example.com/api/");
		Uri relative = new("v2", UriKind.Relative);

		Uri result = baseUri / relative;

		Assert.Equal(new Uri("https://example.com/api/v2"), result);
	}

	[Fact]
	public void SlashOperator_UriAndUri_RightAbsolute_OverridesBase()
	{
		Uri baseUri = new("https://example.com/api/");
		Uri absolute = new("https://other.com/y");

		Uri result = baseUri / absolute;

		Assert.Equal(absolute, result);
	}
}
