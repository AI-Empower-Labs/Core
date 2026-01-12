using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class HostBuilder
{
	public static async Task<THost> Build<THost, THostApplicationBuilder>(
		string[] args,
		Func<string[], THostApplicationBuilder> create,
		Action<THostApplicationBuilder>? configureBuilder,
		Func<THostApplicationBuilder, THost> build,
		Action<THost>? configureHost,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
		where THost : IHost
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		THostApplicationBuilder builder = create(args);
		await builder.AutomaticDependencyInjection(cancellationToken, assemblies);
		configureBuilder?.Invoke(builder);

		THost application = build(builder);
		await application.AutomaticHostSetup<THost>(cancellationToken, assemblies);
		configureHost?.Invoke(application);

		return application;
	}
}
