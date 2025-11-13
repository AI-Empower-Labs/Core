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

	[Fact]
	public void AmpersandOperator_AppendsQuery_WithLeadingQuestionMark()
	{
		Uri baseUri = new("https://example.com/items");
		Uri result = baseUri & "?page=2&sort=asc";

		Assert.Equal(new Uri("https://example.com/items?page=2&sort=asc"), result);
	}

	[Fact]
	public void AmpersandOperator_Concatenates_WhenExistingQueryPresent()
	{
		Uri baseUri = new("https://example.com/items?category=books");
		Uri result = baseUri & "page=1";

		Assert.Equal(new Uri("https://example.com/items?category=books&page=1"), result);
	}

	[Fact]
	public void AmpersandOperator_RespectsInlineFragment()
	{
		Uri baseUri = new("https://example.com/items");
		Uri result = baseUri & "page=1#top";

		Assert.Equal(new Uri("https://example.com/items?page=1#top"), result);
	}

	[Fact]
	public void AmpersandOperator_EmptyString_ReturnsOriginal()
	{
		Uri baseUri = new("https://example.com/items");
		Uri result = baseUri & string.Empty;

		Assert.Same(baseUri, result);
	}

	[Fact]
	public void AmpersandOperator_NullString_ReturnsOriginal()
	{
		Uri baseUri = new("https://example.com/items");
		string? rhs = null;
		// ReSharper disable once ExpressionIsAlwaysNull
		Uri result = baseUri & rhs!;

		Assert.Same(baseUri, result);
	}

	[Fact]
	public void AmpersandOperator_FragmentOnly_PreservesQuery_AndSetsFragment()
	{
		Uri baseUri = new("https://example.com/items?q=abc");
		Uri result = baseUri & "#section1";

		Assert.Equal(new Uri("https://example.com/items?q=abc#section1"), result);
	}

	[Fact]
	public void AmpersandOperator_AppendsQuery_WhenQueryAlreadyPresent()
	{
		Uri baseUri = new("https://example.com/items?q=abc");
		Uri result = baseUri & "q=abc";

		Assert.Equal(new Uri("https://example.com/items?q=abc&q=abc"), result);
	}
}
