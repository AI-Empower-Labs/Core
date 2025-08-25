// ReSharper disable once CheckNamespace

namespace System.IO;

public sealed class ProgressStream(
	Stream stream,
	Action<int>? readProgress,
	Action<int>? writeProgress) : Stream
{
	private int _totalReadBytes;
	private int _totalWriteBytes;

	/// <inheritdoc />
	public override void Flush()
	{
		stream.Flush();
	}

	private void UpdateReadBytes(int change)
	{
		_totalReadBytes += change;
		readProgress?.Invoke(_totalReadBytes);
	}

	private void UpdateWriteBytes(int change)
	{
		_totalWriteBytes += change;
		writeProgress?.Invoke(_totalWriteBytes);
	}

	/// <inheritdoc />
	public override int Read(byte[] buffer, int offset, int count)
	{
		ValidateBufferArgs(buffer, offset, count);

		int totalBytesRead = 0;

		while (totalBytesRead < count)
		{
			int bytesRead = stream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

			if (bytesRead == 0)
			{
				break; // end of stream
			}

			totalBytesRead += bytesRead;
		}

		UpdateReadBytes(totalBytesRead);

		return totalBytesRead;
	}

	/// <inheritdoc />
	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ValidateBufferArgs(buffer, offset, count);

		int totalBytesRead = 0;

		while (totalBytesRead < count)
		{
#if NET || NETSTANDARD2_1
			int bytesRead = await stream.ReadAsync(
				buffer.AsMemory(offset + totalBytesRead, count - totalBytesRead),
				cancellationToken);
#else
                int bytesRead = await stream.ReadAsync(buffer, offset + totalBytesRead, count - totalBytesRead, cancellationToken);
#endif
			if (bytesRead == 0)
			{
				break; // end of stream
			}

			totalBytesRead += bytesRead;
		}

		UpdateReadBytes(totalBytesRead);

		return totalBytesRead;
	}

	/// <inheritdoc />
	public override long Seek(long offset, SeekOrigin origin)
	{
		return stream.Seek(offset, origin);
	}

	/// <inheritdoc />
	public override void SetLength(long value)
	{
		stream.SetLength(value);
	}

	/// <inheritdoc />
	public override void Write(byte[] buffer, int offset, int count)
	{
		ValidateBufferArgs(buffer, offset, count);
		stream.Write(buffer, offset, count);
		UpdateWriteBytes(count);
	}

	/// <inheritdoc />
	public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ValidateBufferArgs(buffer, offset, count);

#if NET || NETSTANDARD2_1
		await stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
#else
            await stream.WriteAsync(buffer, offset, count, cancellationToken);
#endif
		UpdateWriteBytes(count);
	}

	/// <inheritdoc />
	public override bool CanRead => stream.CanRead;

	/// <inheritdoc />
	public override bool CanSeek => stream.CanSeek;

	/// <inheritdoc />
	public override bool CanWrite => stream.CanWrite;

	/// <inheritdoc />
	public override long Length => stream.Length;

	/// <inheritdoc />
	public override long Position
	{
		get => stream.Position;
		set => stream.Position = value;
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			stream.Dispose();
		}

		base.Dispose(disposing);
	}

	/// <summary>
	/// Validates the buffer, offset, and count arguments for read/write calls.
	/// </summary>
	/// <param name="buffer">The buffer to read or write.</param>
	/// <param name="offset">The offset within <paramref name="buffer"/>.</param>
	/// <param name="count">The number of bytes to read or write.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are invalid.</exception>
	/// <exception cref="ArgumentException">Thrown if the sum of <paramref name="offset"/> and <paramref name="count"/> are invalid.</exception>
	private static void ValidateBufferArgs(byte[] buffer, int offset, int count)
	{
#if NET
		ArgumentNullException.ThrowIfNull(buffer);
#else
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
#endif

		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
		}

		if (count < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
		}

		if (offset + count > buffer.Length)
		{
			throw new ArgumentException("The sum of offset and count is greater than the buffer length.", nameof(buffer));
		}
	}
}
