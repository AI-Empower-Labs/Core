namespace AEL.Core.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("a", "A")]
    [InlineData("A", "A")]
    [InlineData("Test", "Test")]
    [InlineData("test", "Test")]
    public void ToPascalCase_ConvertsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, input.ToPascalCase());
    }

    [Fact]
    public void Sanitize_RemovesControlAndEmptyLines()
    {
        string input = "line1\n\nline2\r\nline3\x01\x02\n";
        string expected = "line1\nline2\nline3";
        Assert.Equal(expected, input.Sanitize());
    }

    [Fact]
    public void Sanitize_PreservesLeadingAndTrailingLetterN()
    {
        Assert.Equal("name", "name".Sanitize());
        Assert.Equal("noon", "noon".Sanitize());
    }

    [Fact]
    public void Sanitize_ReturnsEmptyForNullOrEmpty()
    {
        string? nullInput = null;
        string emptyInput = string.Empty;
        Assert.Equal(string.Empty, nullInput.Sanitize());
        Assert.Equal(string.Empty, emptyInput.Sanitize());
    }
}