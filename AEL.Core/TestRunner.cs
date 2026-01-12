using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public sealed class TestRunner<THost>(THost host) : AsyncDisposableBase
	where THost : IHost
{
	public THost Host => host;
}

public static class TestRunner
{
	public static async Task<TestRunner<THost>> Start<THost, THostApplicationBuilder>(
		string[] args,
		Func<string[], THostApplicationBuilder> create,
		Func<THostApplicationBuilder, THost> build,
		params Assembly[] assemblies)
		where THost : IHost
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		CancellationTokenSource cts = new();
		Startup startup = new();
		THost host = await HostBuilder.Build(args, create, build, cts.Token, assemblies);
		await host.StartAsync(cts.Token);
		TestRunner<THost> bag = new(host);
		bag.DisposableBag.Add(cts);
		bag.DisposableBag.Add(startup);
		bag.DisposableBag.Add(async () =>
		{
			await host.StopAsync(cts.Token);
			host.Dispose();
		});
		return bag;
	}
}
