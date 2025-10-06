using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class HostBuilder
{
	public static async Task<THost> Build<THost, THostApplicationBuilder>(
		string[] args,
		Func<string[], THostApplicationBuilder> create,
		Func<THostApplicationBuilder, THost> build,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
		where THost : IHost
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		THostApplicationBuilder builder = create(args);
		await builder.AutomaticDependencyInjection(cancellationToken, assemblies);

		THost application = build(builder);
		await application.AutomaticHostSetup<THost>(cancellationToken, assemblies);

		return application;
	}
}
