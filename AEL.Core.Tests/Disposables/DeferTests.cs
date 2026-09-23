namespace AEL.Core.Tests.Disposables;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public sealed class DeferTests
{
    [Fact]
    public void Defer_SingleAction_ExecutesOnDispose()
    {
        bool executed = false;
        using (new Defer(() => executed = true))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public void Defer_ActionFactory_ExecutesOnDispose()
    {
        bool executed = false;
        using (Defer.Action(() => executed = true))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public void Defer_ActionWithState_ExecutesWithoutClosureAllocation()
    {
        List<string> list = [];
        using (Defer.Action(list, static state => state.Add("done")))
        {
            Assert.Empty(list);
        }

        Assert.Equal(["done"], list);
    }

    [Fact]
    public void Defer_Add_WithState_ExecutesWithoutClosureAllocation()
    {
        List<string> list = [];
        using (Defer defer = new())
        {
            defer.Add(list, static state => state.Add("from-add"));
            Assert.Empty(list);
        }

        Assert.Equal(["from-add"], list);
    }

    [Fact]
    public void Defer_DisposableConstructor_DisposesTarget()
    {
        bool disposed = false;
        DisposableItem item = new(() => disposed = true);

        using (new Defer(item))
        {
            Assert.False(disposed);
        }

        Assert.True(disposed);
    }

    [Fact]
    public void Defer_MultipleActions_ExecutesInLifoOrder()
    {
        List<int> order = [];
        using (Defer defer = new())
        {
            defer.Add(() => order.Add(1));
            defer.Add(() => order.Add(2));
            defer.Add(() => order.Add(3));
        }

        Assert.Equal([3, 2, 1], order);
    }

    [Fact]
    public void Defer_ScopeWithOutDelegate_ExecutesInLifoOrder()
    {
        List<string> order = [];
        using (Defer.Scope(out Action<Action> defer))
        {
            defer(() => order.Add("first"));
            defer(() => order.Add("second"));
            defer(() => order.Add("third"));
        }

        Assert.Equal(["third", "second", "first"], order);
    }

    [Fact]
    public async Task Defer_SupportsAsyncDisposal()
    {
        bool executed = false;
        await using (new Defer(() => executed = true))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public void Defer_AggregatesExceptions_WhenMultipleActionsThrow()
    {
        List<int> executed = [];
        Defer defer = new();
        defer.Add(() =>
        {
            executed.Add(1);
            throw new InvalidOperationException("error 1");
        });
        defer.Add(() =>
        {
            executed.Add(2);
            throw new ArgumentException("error 2");
        });

        AggregateException ex = Assert.Throws<AggregateException>(() => defer.Dispose());
        Assert.Equal(2, ex.InnerExceptions.Count);
        Assert.Contains(ex.InnerExceptions, e => e is ArgumentException);
        Assert.Contains(ex.InnerExceptions, e => e is InvalidOperationException);
        Assert.Equal([2, 1], executed);
    }

    [Fact]
    public void Defer_IsIdempotent_RunsOnlyOnce()
    {
        int count = 0;
        Defer defer = new(() => count++);

        defer.Dispose();
        defer.Dispose();
        defer.Dispose();

        Assert.True(defer.IsDisposed);
        Assert.Equal(1, count);
    }

    [Fact]
    public void Defer_Dismiss_CancelsExecution()
    {
        bool executed = false;
        Defer defer = new(() => executed = true);
        defer.Add(() => executed = true);
        Assert.Equal(2, defer.Count);

        defer.Dismiss();
        Assert.Equal(0, defer.Count);
        defer.Dispose();

        Assert.False(executed);
    }

    [Fact]
    public void Defer_ThrowsArgumentNullException_OnNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new Defer((Action)null!));
        Assert.Throws<ArgumentNullException>(() => new Defer((IDisposable)null!));
        Assert.Throws<ArgumentNullException>(() => Defer.Action(null!));
        Assert.Throws<ArgumentNullException>(() => Defer.Action<object>(null!, null!));

        Defer defer = new();
        Assert.Throws<ArgumentNullException>(() => defer.Add((Action)null!));
        Assert.Throws<ArgumentNullException>(() => defer.Add((IDisposable)null!));
    }

    private sealed class DisposableItem(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}
