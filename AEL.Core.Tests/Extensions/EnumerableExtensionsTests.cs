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

#if NET10_0_OR_GREATER
	[Fact]
	public void JoinOperator_BasicJoin()
	{
		IEnumerable<string> items = ["a", "b", "c"];
		string result = items * ",";
		Assert.Equal("a,b,c", result);
	}

	[Fact]
	public void JoinOperator_EmptySequence_ReturnsEmptyString()
	{
		IEnumerable<string> items = [];
		string result = items * ",";
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void JoinOperator_SingleElement_NoSeparatorInserted()
	{
		IEnumerable<string> items = ["only"];
		string result = items * "|";
		Assert.Equal("only", result);
	}

	[Fact]
	public void JoinOperator_MultiCharacterSeparator()
	{
		IEnumerable<string> items = ["x", "y"];
		string result = items * " - ";
		Assert.Equal("x - y", result);
	}
#endif
}
