namespace AEL.Core.Tests.Disposables;

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public sealed class AsyncDisposableBaseTests
{
    private sealed class DerivedAsyncDisposable : AsyncDisposableBase
    {
        public CancellationToken Token => CancellationToken; // expose
        public AsyncDisposableBag Bag => DisposableBag; // expose
    }

    [Fact]
    public async Task DisposeAsync_CancelsToken_IfCreated_And_DisposesBag()
    {
        DerivedAsyncDisposable d = new();
        bool innerDisposed = false;
        d.Bag.Add(() =>
        {
            innerDisposed = true;
            return Task.CompletedTask;
        });

        CancellationToken token = d.Token;
        Assert.False(token.IsCancellationRequested);

        await d.DisposeAsync();

        Assert.True(d.IsDisposed);
        Assert.True(innerDisposed);
        Assert.True(token.IsCancellationRequested);

        // Idempotent
        await d.DisposeAsync();
        Assert.True(d.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_WhenBagThrows_StillCancelsToken()
    {
        DerivedAsyncDisposable d = new();
        d.Bag.Add(() => throw new InvalidOperationException("Cleanup error"));
        CancellationToken token = d.Token;

        await Assert.ThrowsAsync<AggregateException>(async () => await d.DisposeAsync());
        Assert.True(d.IsDisposed);
        Assert.True(token.IsCancellationRequested);
    }
}
