namespace AEL.Core.Tests;

public sealed class FileNameHelperTests
{
	[Fact]
	public void SanitizeFileName_ValidName_ReturnsUnchanged()
	{
		const string valid = "report_2026-09-03.pdf";
		Assert.Equal(valid, FileNameHelper.SanitizeFileName(valid));
	}

	[Fact]
	public void SanitizeFileName_NullOrEmpty_ReturnsOriginal()
	{
		Assert.Null(FileNameHelper.SanitizeFileName(null!));
		Assert.Equal(string.Empty, FileNameHelper.SanitizeFileName(string.Empty));
	}

	[Fact]
	public void SanitizeFileName_InvalidChars_ReplacedWithUnderscore()
	{
		char[] invalidChars = Path.GetInvalidFileNameChars();
		string input = $"file{invalidChars[0]}name{invalidChars[^1]}.txt";
		string sanitized = FileNameHelper.SanitizeFileName(input);

		Assert.DoesNotContain(invalidChars[0], sanitized);
		Assert.DoesNotContain(invalidChars[^1], sanitized);
		Assert.Equal("file_name_.txt", sanitized);
	}
}
