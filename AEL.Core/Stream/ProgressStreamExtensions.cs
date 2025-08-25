using System.Threading.Channels;

namespace System.IO;

public static class ProgressStreamExtensions
{
	public static ProgressStream AsProgressStream(this Stream stream, IProgress<int>? progress = null)
	{
		return new ProgressStream(stream, progress, progress);
	}

	public static ProgressStream AsReadProgressStream(this Stream stream, IProgress<int>? progress = null)
	{
		return new ProgressStream(stream, progress);
	}

	public static ProgressStream AsWriteProgressStream(this Stream stream, IProgress<int>? progress = null)
	{
		return new ProgressStream(stream, null, progress);
	}

	public static ProgressStream AsReadProgressStream(this Stream stream, Channel<int> progress)
	{
		return new ProgressStream(stream, new ProgressChannelWriter(progress));
	}

	public static ProgressStream AsWriteProgressStream(this Stream stream, Channel<int> progress)
	{
		return new ProgressStream(stream, null, new ProgressChannelWriter(progress));
	}

	private sealed class ProgressChannelWriter(Channel<int> channel) : IProgress<int>
	{
		public void Report(int value)
		{
			channel.Writer.TryWrite(value);
		}
	}
}
