namespace AEL.Core.Tests;

public sealed class NanoidTests
{
	[Fact]
	public void Generate_DefaultSize_Is21Chars()
	{
		string id = Nanoid.Generate();
		Assert.Equal(21, id.Length);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(10)]
	[InlineData(32)]
	[InlineData(64)]
	public void Generate_CustomSize_MatchesRequestedLength(int size)
	{
		string id = Nanoid.Generate(size: size);
		Assert.Equal(size, id.Length);
	}

	[Fact]
	public void Generate_CustomAlphabet_UsesOnlySpecifiedChars()
	{
		const string alphabet = "abc123";
		string id = Nanoid.Generate(alphabet: alphabet, size: 50);

		Assert.Equal(50, id.Length);
		Assert.All(id, c => Assert.Contains(c, alphabet));
	}

	[Fact]
	public void Generate_GeneratesUniqueIds()
	{
		HashSet<string> ids = [];
		for (int i = 0; i < 500; i++)
		{
			Assert.True(ids.Add(Nanoid.Generate()));
		}
	}

	[Fact]
	public void Generate_InvalidArguments_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => Nanoid.Generate(alphabet: null!));
		Assert.Throws<ArgumentOutOfRangeException>(() => Nanoid.Generate(alphabet: ""));
		Assert.Throws<ArgumentOutOfRangeException>(() => Nanoid.Generate(size: 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => Nanoid.Generate(size: -5));
	}

	[Fact]
	public void CryptoRandom_NextRange_ProducesValuesWithinBounds()
	{
		CryptoRandom random = new();
		for (int i = 0; i < 100; i++)
		{
			int value = random.Next(10, 20);
			Assert.InRange(value, 10, 19);
		}
	}
}
