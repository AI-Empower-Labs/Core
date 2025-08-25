namespace System.IO;

public static class ProgressStreamExtensions
{
	public static Stream AsReadProgressStream(this Stream stream, Action<int> progress)
	{
		return new ProgressStream(stream, progress, null);
	}

	public static Stream AsWriteProgressStream(this Stream stream, Action<int> progress)
	{
		return new ProgressStream(stream, null, progress);
	}
}
