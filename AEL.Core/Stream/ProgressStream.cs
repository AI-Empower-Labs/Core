// ReSharper disable once CheckNamespace

namespace System.IO;

public sealed class ProgressStream(
	Stream stream,
	Action<long>? readProgress,
	Action<long>? writeProgress) : Stream
{
	private long _totalReadBytes;
	private long _totalWriteBytes;

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
		ValidateBufferArguments(buffer, offset, count);
		int bytesRead = stream.Read(buffer, offset, count);
		UpdateReadBytes(bytesRead);
		return bytesRead;
	}

	/// <inheritdoc />
	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ValidateBufferArguments(buffer, offset, count);
		int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
		UpdateReadBytes(bytesRead);
		return bytesRead;
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
		ValidateBufferArguments(buffer, offset, count);
		stream.Write(buffer, offset, count);
		UpdateWriteBytes(count);
	}

	/// <inheritdoc />
	public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		ValidateBufferArguments(buffer, offset, count);
		await stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
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
}
