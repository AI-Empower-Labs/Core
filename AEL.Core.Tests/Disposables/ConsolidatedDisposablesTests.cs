namespace AEL.Core.Tests.Disposables;

using System;
using System.Threading.Tasks;
using Xunit;

public sealed class ConsolidatedDisposablesTests
{
    [Fact]
    public void DisposableBag_Is_Defer_And_CanBeUsedPolymorphically()
    {
        DisposableBag bag = new();
        Assert.True(bag is not null);
        Assert.True(bag is not null);
        Assert.True(bag is not null);

        Defer defer = bag;
        bool executed = false;
        defer.Add(() => executed = true);
        defer.Dispose();

        Assert.True(executed);
        Assert.True(bag.IsDisposed);
    }

    [Fact]
    public async Task AsyncDisposableBag_Is_AsyncDefer_And_CanBeUsedPolymorphically()
    {
        AsyncDisposableBag bag = new();
        Assert.True(bag is not null);
        Assert.True(bag is not null);

        AsyncDefer defer = bag;
        bool executed = false;
        defer.Add(async () =>
        {
            await Task.Yield();
            executed = true;
        });
        await defer.DisposeAsync(TestContext.Current.CancellationToken);

        Assert.True(executed);
        Assert.True(bag.IsDisposed);
    }

    [Fact]
    public async Task DeferAsync_Is_AsyncDefer_And_WorksAcrossAllConstructors()
    {
        bool a1 = false, a2 = false, a3 = false;

        DeferAsync d1 = new(() => { a1 = true; });
        await d1.DisposeAsync(TestContext.Current.CancellationToken);
        Assert.True(a1);

        DeferAsync d2 = new(async () => { await Task.Yield(); a2 = true; });
        await d2.DisposeAsync(TestContext.Current.CancellationToken);
        Assert.True(a2);

        DeferAsync d3 = new(async ct => { await Task.Yield(); a3 = true; });
        await d3.DisposeAsync(TestContext.Current.CancellationToken);
        Assert.True(a3);
    }

    [Fact]
    public async Task Defer_Supports_AsyncDisposal()
    {
        bool executed = false;
        await using (new Defer(() => executed = true))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public void DeferBase_Dismiss_And_Count_WorkConsistently()
    {
        Defer defer = new();
        Assert.Equal(0, defer.Count);
        defer.Add(() => { });
        defer.Add(() => { });
        Assert.Equal(2, defer.Count);

        defer.Dismiss();
        Assert.Equal(0, defer.Count);
        defer.Dispose();
        Assert.True(defer.IsDisposed);

        AsyncDefer asyncDefer = new();
        Assert.Equal(0, asyncDefer.Count);
        asyncDefer.Add(() => { });
        asyncDefer.Add(async () => await Task.Yield());
        Assert.Equal(2, asyncDefer.Count);

        asyncDefer.Dismiss();
        Assert.Equal(0, asyncDefer.Count);
    }

    [Fact]
    public async Task Disposables_DeferAsync_Returns_DeferAsync()
    {
        bool executed = false;
        await using (DeferAsync defer = Disposables.DeferAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        }))
        {
            Assert.IsType<DeferAsync>(defer);
        }

        Assert.True(executed);
    }
}
