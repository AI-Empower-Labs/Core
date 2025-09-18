namespace AEL.Core.Stream;

public static class ProgressHelpers
{
	public static (double Percent, double BitsPerSecond, double BytesPerSecond) CalculateProgress(
		int bytesSoFar,
		int lastBytesSoFar,
		long totalBytes,
		TimeSpan timeSinceLast)
	{
		double getBytesPerSecond = (bytesSoFar - lastBytesSoFar) / timeSinceLast.TotalSeconds;
		double bitsPerSecond = getBytesPerSecond * 8;
		double percent = (double)100 * bytesSoFar / totalBytes;
		return (percent, bitsPerSecond, getBytesPerSecond);
	}
}
