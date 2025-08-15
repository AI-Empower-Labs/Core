namespace AEL.Core.Tests.Disposables;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public sealed class AsyncDisposableBagTests
{
    [Fact]
    public async Task Add_Action_And_Task_And_Disposables_ExecutesInLifoOrder_OnDisposeAsync()
    {
        List<string> order = [];
        AsyncDisposableBag bag = new();

        bag.Add(new TestAsyncDisposable(async () =>
        {
            order.Add("iad");
            await Task.Delay(1);
        }));
        bag.Add(new TestDisposable(() => order.Add("id")));
        bag.Add(() => order.Add("action"));
        bag.Add(async () =>
        {
            await Task.Delay(1);
            order.Add("task");
        });

        await bag.DisposeAsync();

        Assert.True(bag.IsDisposed);
        Assert.Equal(new[] { "task", "action", "id", "iad" }, order);
    }

    private sealed class TestDisposable(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }

    private sealed class TestAsyncDisposable(Func<Task> onDisposeAsync) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await onDisposeAsync();
        }
    }

    [Fact]
    public async Task DisposeAsync_AggregatesExceptions()
    {
        AsyncDisposableBag bag = new();
        bag.Add(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("one");
        });
        bag.Add(async () =>
        {
            await Task.Yield();
            throw new ApplicationException("two");
        });

        AggregateException ex = await Assert.ThrowsAsync<AggregateException>(async () => await bag.DisposeAsync());
        Assert.Equal(2, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent_RunsOnce()
    {
        int runs = 0;
        AsyncDisposableBag bag = new();
        bag.Add(() => runs++);

        await bag.DisposeAsync();
        await bag.DisposeAsync();

        Assert.True(bag.IsDisposed);
        Assert.Equal(1, runs);
    }
}