namespace AEL.Core.Tests.Stream;

public sealed class ProgressStreamTests
{
	[Fact]
	public async Task Properties()
	{
		System.IO.Stream inputStream = new MemoryStream(new byte[10]);
		await using ProgressStream stream = new(inputStream, null, null);

		Assert.Equal(inputStream.CanRead, stream.CanRead);
		Assert.Equal(inputStream.CanSeek, stream.CanSeek);
		Assert.Equal(inputStream.CanWrite, stream.CanWrite);
		Assert.Equal(inputStream.Length, stream.Length);

		inputStream.Position = 1;

		Assert.Equal(inputStream.Position, stream.Position);

		stream.Position = 2;

		Assert.Equal(inputStream.Position, stream.Position);
	}

	[Fact]
	public async Task Flush()
	{
		byte[] buffer = new byte[1024760];

		System.IO.Stream inputStream = new MemoryStream();

		int bytesReadOverall = 0;
		void WriteProgress(int bytesRead)
		{
			bytesReadOverall += bytesRead;
		}

		await using MemoryStream stream = new(new byte[1_000_000]);
		await using ProgressStream outputStream = new(inputStream, null, writeProgress: WriteProgress);

		while (true)
		{
			int bytesRead = stream.Read(buffer, 0, buffer.Length);

			if (bytesRead == 0)
			{
				break;
			}

			await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), TestContext.Current.CancellationToken);
		}

		await outputStream.FlushAsync(TestContext.Current.CancellationToken);

		Assert.Equal(inputStream.Length, bytesReadOverall);
	}

	[Fact]
	public async Task Seek()
	{
		System.IO.Stream inputStream = new MemoryStream(new byte[10]);

		await using ProgressStream stream = new(inputStream, null, null);

		stream.Seek(5, SeekOrigin.Begin);

		Assert.Equal(5, inputStream.Position);
		Assert.Equal(5, stream.Position);
	}

	[Fact]
	public async Task SetLength()
	{
		System.IO.Stream inputStream = new MemoryStream(new byte[10]);

		await using ProgressStream stream = new(inputStream, null, null);

		stream.SetLength(5);

		Assert.Equal(5, inputStream.Length);
		Assert.Equal(5, stream.Length);
	}

	[Fact]
	public async Task Read()
	{
		byte[] buffer = new byte[1024760];

		System.IO.Stream inputStream = new MemoryStream(new byte[1_000_000]);

		int bytesReadOverall = 0;
		void WriteProgress(int bytesRead)
		{
			bytesReadOverall = bytesRead;
		}

		await using ProgressStream stream = new(inputStream, WriteProgress, null);
		await using MemoryStream outputStream = new();

		while (true)
		{
			int bytesRead = stream.Read(buffer, 0, buffer.Length);

			if (bytesRead == 0)
			{
				break;
			}

			await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), TestContext.Current.CancellationToken);
		}

		Assert.Equal(inputStream.Length, bytesReadOverall);
	}

	[Fact]
	public async Task ReadAsync()
	{
		byte[] buffer = new byte[1024760];

		System.IO.Stream inputStream = new MemoryStream(new byte[1_000_000]);

		int bytesReadOverall = 0;
		void WriteProgress(int bytesRead)
		{
			bytesReadOverall = bytesRead;
		}

		await using ProgressStream stream = new(inputStream, WriteProgress, null);
		await using MemoryStream outputStream = new();

		while (true)
		{
			int bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);

			if (bytesRead == 0)
			{
				break;
			}

			await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), TestContext.Current.CancellationToken);
		}

		Assert.Equal(inputStream.Length, bytesReadOverall);
	}

	[Fact]
	public void Read_PreservesShortReads()
	{
		byte[] source = [1, 2, 3, 4];
		using OneByteAtATimeStream inner = new(source);
		using ProgressStream stream = new(inner, null, null);

		byte[] buffer = new byte[8];
		int bytesRead = stream.Read(buffer, 0, buffer.Length);

		Assert.Equal(1, bytesRead);
		Assert.Equal(1, buffer[0]);
		Assert.Equal(1, inner.Position);
	}

	[Fact]
	public async Task ReadAsync_PreservesShortReads()
	{
		byte[] source = [1, 2, 3, 4];
		await using OneByteAtATimeStream inner = new(source);
		await using ProgressStream stream = new(inner, null, null);

		byte[] buffer = new byte[8];
		int bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);

		Assert.Equal(1, bytesRead);
		Assert.Equal(1, buffer[0]);
		Assert.Equal(1, inner.Position);
	}

	[Fact]
	public async Task Write()
	{
		byte[] buffer = new byte[1024760];

		System.IO.Stream inputStream = new MemoryStream();

		int bytesReadOverall = 0;
		void WriteProgress(int bytesRead)
		{
			bytesReadOverall += bytesRead;
		}

		await using MemoryStream stream = new(new byte[1_000_000]);
		await using ProgressStream outputStream = new(inputStream, null, WriteProgress);

		while (true)
		{
			int bytesRead = stream.Read(buffer, 0, buffer.Length);

			if (bytesRead == 0)
			{
				break;
			}

			outputStream.Write(buffer, 0, bytesRead);
		}

		Assert.Equal(inputStream.Length, bytesReadOverall);
	}

	[Fact]
	public async Task WriteAsync()
	{
		byte[] buffer = new byte[1024760];

		System.IO.Stream inputStream = new MemoryStream();

		int bytesReadOverall = 0;
		void WriteProgress(int bytesRead)
		{
			bytesReadOverall += bytesRead;
		}

		await using MemoryStream stream = new(new byte[1_000_000]);
		await using ProgressStream outputStream = new(inputStream, null, WriteProgress);

		while (true)
		{
			int bytesRead = stream.Read(buffer, 0, buffer.Length);

			if (bytesRead == 0)
			{
				break;
			}

			await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), TestContext.Current.CancellationToken);
		}

		Assert.Equal(inputStream.Length, bytesReadOverall);
	}

	private sealed class OneByteAtATimeStream(byte[] data) : System.IO.Stream
	{
		private int _position;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => data.Length;

		public override long Position
		{
			get => _position;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count == 0 || _position >= data.Length)
			{
				return 0;
			}

			buffer[offset] = data[_position++];
			return 1;
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return Task.FromResult(Read(buffer, offset, count));
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
