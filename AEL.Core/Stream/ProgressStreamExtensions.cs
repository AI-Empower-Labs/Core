using Nerdbank.Streams;

namespace System.IO;

public static class ProgressStreamExtensions
{
	public static Stream AsReadProgressStream(this Stream stream, Action<int> progress)
	{
		MonitoringStream monitoringStream = new(stream);
		monitoringStream.DidReadAny += (_, segment) => progress(segment.Length);
		return monitoringStream;
	}

	public static Stream AsWriteProgressStream(this Stream stream, Action<int> progress)
	{
		MonitoringStream monitoringStream = new(stream);
		monitoringStream.DidWriteAny += (_, segment) => progress(segment.Length);
		return monitoringStream;
	}
}
