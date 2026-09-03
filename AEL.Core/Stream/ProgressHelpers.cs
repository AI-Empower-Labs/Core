namespace AEL.Core.Stream;

public static class ProgressHelpers
{
	public static (double Percent, double BitsPerSecond, double BytesPerSecond) CalculateProgress(
		long bytesSoFar,
		long lastBytesSoFar,
		long totalBytes,
		TimeSpan timeSinceLast)
	{
		double seconds = timeSinceLast.TotalSeconds;
		double getBytesPerSecond = seconds > 0 ? (bytesSoFar - lastBytesSoFar) / seconds : 0;
		double bitsPerSecond = getBytesPerSecond * 8;
		double percent = totalBytes > 0 ? (double)100 * bytesSoFar / totalBytes : 0;
		return (percent, bitsPerSecond, getBytesPerSecond);
	}
}
