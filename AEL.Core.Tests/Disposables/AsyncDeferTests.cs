namespace AEL.Core.Tests.Disposables;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public sealed class AsyncDeferTests
{
    [Fact]
    public async Task AsyncDefer_SingleAsyncAction_ExecutesOnDisposeAsync()
    {
        bool executed = false;
        await using (new AsyncDefer(async () =>
        {
            await Task.Yield();
            executed = true;
        }))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public async Task AsyncDefer_ActionFactory_ExecutesOnDisposeAsync()
    {
        bool executed = false;
        await using (AsyncDefer.Action(async () =>
        {
            await Task.Yield();
            executed = true;
        }))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public async Task AsyncDefer_RunFactory_ExecutesOnDisposeAsync()
    {
        bool executed = false;
        await using (AsyncDefer.Run(async () =>
        {
            await Task.Yield();
            executed = true;
        }))
        {
            Assert.False(executed);
        }

        Assert.True(executed);
    }

    [Fact]
    public async Task AsyncDefer_ActionWithState_ExecutesOnDisposeAsync()
    {
        List<string> list = [];
        await using (AsyncDefer.Action(list, static async (state, _) =>
        {
            await Task.Yield();
            state.Add("done");
        }))
        {
            Assert.Empty(list);
        }

        Assert.Equal(["done"], list);
    }

    [Fact]
    public async Task AsyncDefer_Add_WithState_ExecutesOnDisposeAsync()
    {
        List<string> list = [];
        await using (AsyncDefer defer = new())
        {
            defer.Add(list, static async (state, _) =>
            {
                await Task.Yield();
                state.Add("task-ct");
            });
            defer.Add(list, static async state =>
            {
                await Task.Yield();
                state.Add("task");
            });
            defer.Add(list, static state => state.Add("sync"));
            defer.Add(static (List<string> state, CancellationToken _) =>
            {
                state.Add("optional-task-ct");
                return Task.CompletedTask;
            }, list);
            defer.Add(static (List<string> state) =>
            {
                state.Add("optional-task");
                return Task.CompletedTask;
            }, list);
            defer.Add(static (List<string> state) => state.Add("optional-sync"), list);

            Assert.Empty(list);
        }

        Assert.Equal(["optional-sync", "optional-task", "optional-task-ct", "sync", "task", "task-ct"], list);
    }

    [Fact]
    public async Task AsyncDefer_MultipleActions_ExecutesInLifoOrder()
    {
        List<int> order = [];
        await using (AsyncDefer defer = new())
        {
            defer.Add(async () =>
            {
                await Task.Yield();
                order.Add(1);
            });
            defer.Add(() => order.Add(2));
            defer.Add(async ct =>
            {
                await Task.Yield();
                order.Add(3);
            });
        }

        Assert.Equal([3, 2, 1], order);
    }

    [Fact]
    public async Task AsyncDefer_ScopeWithOutDelegate_ExecutesInLifoOrder()
    {
        List<string> order = [];
        await using (AsyncDefer.Scope(out Action<Func<Task>> defer))
        {
            defer(async () =>
            {
                await Task.Yield();
                order.Add("first");
            });
            defer(async () =>
            {
                await Task.Yield();
                order.Add("second");
            });
        }

        Assert.Equal(["second", "first"], order);
    }

    [Fact]
    public async Task AsyncDefer_ScopeMethod_ExecutesInLifoOrder()
    {
        List<int> order = [];
        await using (AsyncDefer defer = AsyncDefer.Scope())
        {
            defer.Add(async () =>
            {
                await Task.Yield();
                order.Add(10);
            });
            defer.Add(async () =>
            {
                await Task.Yield();
                order.Add(20);
            });
        }

        Assert.Equal([20, 10], order);
    }

    [Fact]
    public async Task AsyncDefer_DisposesSyncAndAsyncDisposables()
    {
        bool syncDisposed = false;
        bool asyncDisposed = false;
        SyncItem sync = new(() => syncDisposed = true);
        AsyncItem async = new(() => asyncDisposed = true);

        await using (AsyncDefer defer = new())
        {
            defer.Add(sync);
            defer.Add(async);
        }

        Assert.True(syncDisposed);
        Assert.True(asyncDisposed);
    }

    [Fact]
    public async Task AsyncDefer_AggregatesExceptions_WhenMultipleActionsThrow()
    {
        List<int> executed = [];
        AsyncDefer defer = new();
        defer.Add(async () =>
        {
            await Task.Yield();
            executed.Add(1);
            throw new InvalidOperationException("error 1");
        });
        defer.Add(async () =>
        {
            await Task.Yield();
            executed.Add(2);
            throw new ArgumentException("error 2");
        });

        AggregateException ex = await Assert.ThrowsAsync<AggregateException>(async () => await defer.DisposeAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, ex.InnerExceptions.Count);
        Assert.Contains(ex.InnerExceptions, e => e is ArgumentException);
        Assert.Contains(ex.InnerExceptions, e => e is InvalidOperationException);
        Assert.Equal([2, 1], executed);
    }

    [Fact]
    public async Task AsyncDefer_IsIdempotent_RunsOnlyOnce()
    {
        int count = 0;
        AsyncDefer defer = new(async () =>
        {
            await Task.Yield();
            count++;
        });

        await defer.DisposeAsync(TestContext.Current.CancellationToken);
        await defer.DisposeAsync(TestContext.Current.CancellationToken);
        await defer.DisposeAsync(TestContext.Current.CancellationToken);

        Assert.True(defer.IsDisposed);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AsyncDefer_Clear_CancelsExecution()
    {
        bool executed = false;
        AsyncDefer defer = new(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        defer.Clear();
        await defer.DisposeAsync(TestContext.Current.CancellationToken);

        Assert.False(executed);
    }

    [Fact]
    public async Task AsyncDefer_Dismiss_CancelsExecution()
    {
        bool executed = false;
        AsyncDefer defer = new(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        defer.Dismiss();
        await defer.DisposeAsync(TestContext.Current.CancellationToken);

        Assert.False(executed);
    }

    [Fact]
    public async Task AsyncDefer_PropagatesCancellationToken()
    {
        CancellationToken observedToken = default;
        using CancellationTokenSource cts = new();

        AsyncDefer defer = new(ct =>
        {
            observedToken = ct;
            return Task.CompletedTask;
        });

        await defer.DisposeAsync(cts.Token);
        Assert.Equal(cts.Token, observedToken);
    }

    [Fact]
    public void AsyncDefer_ThrowsArgumentNullException_OnNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new AsyncDefer((Func<Task>)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncDefer((Func<CancellationToken, Task>)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncDefer((Action)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncDefer((IAsyncDisposable)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncDefer((IDisposable)null!));
        Assert.Throws<ArgumentNullException>(() => AsyncDefer.Action((Func<Task>)null!));
        Assert.Throws<ArgumentNullException>(() => AsyncDefer.Run((Func<Task>)null!));

        AsyncDefer defer = new();
        Assert.Throws<ArgumentNullException>(() => defer.Add((Func<Task>)null!));
        Assert.Throws<ArgumentNullException>(() => defer.Add((Func<CancellationToken, Task>)null!));
        Assert.Throws<ArgumentNullException>(() => defer.Add((Action)null!));
        Assert.Throws<ArgumentNullException>(() => defer.Add((IAsyncDisposable)null!));
        Assert.Throws<ArgumentNullException>(() => defer.Add((IDisposable)null!));
    }

    [Fact]
    public async Task Disposables_DeferAsync_HelperMethods_Work()
    {
        bool executed1 = false;
        await using (Disposables.DeferAsync(async () =>
        {
            await Task.Yield();
            executed1 = true;
        }))
        {
            Assert.False(executed1);
        }
        Assert.True(executed1);

        List<int> order = [];
        await using (AsyncDefer defer = Disposables.DeferAsync())
        {
            defer.Add(() => order.Add(1));
            defer.Add(() => order.Add(2));
        }
        Assert.Equal([2, 1], order);
    }

    [Fact]
    public async Task Defer_Async_StaticBridge_Works()
    {
        bool executed = false;
        await using (Defer.Async(async () =>
        {
            await Task.Yield();
            executed = true;
        }))
        {
            Assert.False(executed);
        }
        Assert.True(executed);

        List<int> order = [];
        await using (AsyncDefer defer = Defer.AsyncScope())
        {
            defer.Add(() => order.Add(1));
            defer.Add(() => order.Add(2));
        }
        Assert.Equal([2, 1], order);
    }

    private sealed class SyncItem(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }

    private sealed class AsyncItem(Action onDispose) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            onDispose();
            return ValueTask.CompletedTask;
        }
    }
}
