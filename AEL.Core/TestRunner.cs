using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class ConsoleTestRunner
{
	public static Task<IAsyncDisposable> Start(
		string[] args,
		Action<IHostApplicationBuilder>? configureBuilder = null,
		Action<IHost>? configureApplication = null,
		params Assembly[] assemblies)
	{
		return TestRunner.Start<IHost, HostApplicationBuilder>(
			args,
			strings =>
			{
				HostApplicationBuilder builder = new(strings);
				configureBuilder?.Invoke(builder);
				return builder;
			},
			hostApplicationBuilder =>
			{
				IHost host = hostApplicationBuilder.Build();
				configureApplication?.Invoke(host);
				return host;
			},
			assemblies);
	}
}

public static class TestRunner
{
	public static async Task<IAsyncDisposable> Start<THost, THostApplicationBuilder>(
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
		AsyncDisposableBag bag = new();
		bag.Add(cts);
		bag.Add(startup);
		bag.Add(host);
		bag.Add(async () =>
		{
			await host.StopAsync(cts.Token);
			host.Dispose();
		});
		return bag;
	}
}
