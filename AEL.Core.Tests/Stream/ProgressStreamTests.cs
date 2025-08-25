namespace AEL.Core.Tests.Stream;

public sealed class ProgressStreamTests
{
	#pragma warning disable CA1852
	private class UnitTestProgress<T>(Action<T> handler) : IProgress<T>
	{
		public void Report(T value)
		{
			handler(value);
		}
	}

	[Fact]
	public async Task Properties()
	{
		System.IO.Stream inputStream = new MemoryStream(new byte[10]);

		await using ProgressStream stream = new(inputStream);

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
		UnitTestProgress<int> writeProgress = new(bytesRead =>
		{
			bytesReadOverall += bytesRead;
		});

		await using MemoryStream stream = new(new byte[1_000_000]);
		await using ProgressStream outputStream = new(inputStream, writeProgress: writeProgress);

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

		await using ProgressStream stream = new(inputStream);

		stream.Seek(5, SeekOrigin.Begin);

		Assert.Equal(5, inputStream.Position);
		Assert.Equal(5, stream.Position);
	}

	[Fact]
	public async Task SetLength()
	{
		System.IO.Stream inputStream = new MemoryStream(new byte[10]);

		await using ProgressStream stream = new(inputStream);

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
		UnitTestProgress<int> readProgress = new(bytesRead =>
		{
			bytesReadOverall += bytesRead;
		});

		await using ProgressStream stream = new(inputStream, readProgress);
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
		UnitTestProgress<int> readProgress = new(bytesRead =>
		{
			bytesReadOverall += bytesRead;
		});

		await using ProgressStream stream = new(inputStream, readProgress);
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
	public async Task Write()
	{
		byte[] buffer = new byte[1024760];

		System.IO.Stream inputStream = new MemoryStream();

		int bytesReadOverall = 0;
		UnitTestProgress<int> writeProgress = new(bytesRead =>
		{
			bytesReadOverall += bytesRead;
		});

		await using MemoryStream stream = new(new byte[1_000_000]);
		await using ProgressStream outputStream = new(inputStream, writeProgress: writeProgress);

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
		UnitTestProgress<int> writeProgress = new(bytesRead =>
		{
			bytesReadOverall += bytesRead;
		});

		await using MemoryStream stream = new(new byte[1_000_000]);
		await using ProgressStream outputStream = new(inputStream, writeProgress: writeProgress);

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
}
