namespace AEL.Core.Tests.Disposables;

public sealed class AsyncCompletionScopeTests
{
	private sealed class AsyncDisposableItem : IAsyncDisposable
	{
		public bool Disposed { get; private set; }

		public ValueTask DisposeAsync()
		{
			Disposed = true;
			return ValueTask.CompletedTask;
		}
	}

	private sealed class SyncDisposableItem : IDisposable
	{
		public bool Disposed { get; private set; }

		public void Dispose()
		{
			Disposed = true;
		}
	}

	[Fact]
	public async Task DisposeAsync_AwaitsTasksAndDisposesAllResources()
	{
		bool taskRan = false;
		bool valueTaskRan = false;
		SyncDisposableItem syncItem = new();
		AsyncDisposableItem asyncItem = new();
		MemoryStream stream = new();

		await using (AsyncCompletionScope scope = new())
		{
			scope.Add(Task.Run(() => taskRan = true));
			scope.Add(new ValueTask(Task.Run(() => valueTaskRan = true)));
			scope.Add(syncItem);
			scope.Add(asyncItem);
			scope.Add(stream);
		}

		Assert.True(taskRan);
		Assert.True(valueTaskRan);
		Assert.True(syncItem.Disposed);
		Assert.True(asyncItem.Disposed);
		Assert.False(stream.CanRead); // Disposed stream
	}
}
