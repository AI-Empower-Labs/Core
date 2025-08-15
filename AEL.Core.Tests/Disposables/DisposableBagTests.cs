namespace AEL.Core.Tests.Disposables;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public sealed class DisposableBagTests
{
    [Fact]
    public void Add_Action_ExecutesInLifoOrder_OnDispose()
    {
        List<int> order = [];
        DisposableBag bag = new();
        bag.Add(() => order.Add(1));
        bag.Add(() => order.Add(2));
        bag.Add(() => order.Add(3));

        bag.Dispose();

        Assert.True(bag.IsDisposed);
        Assert.Equal(new[] { 3, 2, 1 }, order);
    }

    private sealed class TestDisposable(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }

    [Fact]
    public void Add_IDisposable_DisposesOnDispose()
    {
        int count = 0;
        DisposableBag bag = new();
        bag.Add(new TestDisposable(() => count++));
        bag.Add(new TestDisposable(() => count++));

        bag.Dispose();

        Assert.Equal(2, count);
    }

    [Fact]
    public void Dispose_AggregatesExceptions()
    {
        DisposableBag bag = new();
        bag.Add(() => throw new InvalidOperationException("one"));
        bag.Add(() => throw new ApplicationException("two"));

        AggregateException ex = Assert.Throws<AggregateException>(() => bag.Dispose());
        Assert.Equal(2, ex.InnerExceptions.Count);
        Assert.Contains(ex.InnerExceptions, e => e is InvalidOperationException && e.Message.Contains("one"));
        Assert.Contains(ex.InnerExceptions, e => e is ApplicationException && e.Message.Contains("two"));
    }

    [Fact]
    public void Dispose_IsIdempotent_RunsOnce()
    {
        int runs = 0;
        DisposableBag bag = new();
        bag.Add(() => runs++);

        bag.Dispose();
        bag.Dispose();
        bag.Dispose();

        Assert.True(bag.IsDisposed);
        Assert.Equal(1, runs);
    }
}