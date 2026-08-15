namespace AEL.Core.Tests;

public sealed class TempFileTests
{
	[Fact]
	public void DefaultConstructor_CreatesAndDeletesTempFile()
	{
		string fileName;
		using (TempFile tempFile = new())
		{
			fileName = tempFile.FileName;
			Assert.True(File.Exists(fileName));
		}

		Assert.False(File.Exists(fileName));
	}

	[Fact]
	public void NamedConstructor_UsesProvidedPath_AndDeletesOnlyThatFile()
	{
		string directory = Path.Combine(Path.GetTempPath(), "ael-tempfile-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(directory);
		try
		{
			string fileName = Path.Combine(directory, "named.bin");
			File.WriteAllText(fileName, "content");

			using (TempFile tempFile = new(fileName))
			{
				Assert.Equal(fileName, tempFile.FileName);
				Assert.True(File.Exists(fileName));
			}

			Assert.False(File.Exists(fileName));
			Assert.Empty(Directory.GetFiles(directory));
		}
		finally
		{
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
			}
		}
	}
}
