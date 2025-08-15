namespace AEL.Core.Tests.Extensions;

public class EnumerableExtensionsTests
{
    [Fact]
    public void WhereNotNull_FiltersNulls()
    {
        IEnumerable<string?> items = ["a", null, "b"];
        IEnumerable<string> result = items.WhereNotNull();
        Assert.Equal(new[] { "a", "b" }, result.ToArray());
    }

    [Fact]
    public async Task WhereNotNull_Async_FiltersNulls()
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async IAsyncEnumerable<string?> GetItems()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            yield return "x";
            yield return null;
            yield return "y";
        }

        IAsyncEnumerable<string> filtered = GetItems().WhereNotNull();
        List<string> list = [];
        await foreach (string s in filtered)
        {
            list.Add(s);
        }

        Assert.Equal(new[] { "x", "y" }, list);
    }
}