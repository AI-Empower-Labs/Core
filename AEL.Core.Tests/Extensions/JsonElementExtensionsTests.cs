namespace AEL.Core.Tests.Extensions;

using System.Text.Json;
using Xunit;

public sealed class JsonElementExtensionsTests
{
    [Fact]
    public void SafeDeserialize_InvalidJson_ReturnsDefault()
    {
        using JsonDocument document = JsonDocument.Parse("\"not a number\"");

        Assert.Equal(0, document.RootElement.SafeDeserialize<int>(null));
        Assert.Equal(0, document.SafeDeserialize<int>(null));
    }

    [Fact]
    public void SafeDeserialize_ValidJson_ReturnsValue()
    {
        using JsonDocument document = JsonDocument.Parse("42");

        Assert.Equal(42, document.SafeDeserialize<int>(null));
    }
}
