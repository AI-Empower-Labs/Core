using System.Reflection;

using Microsoft.Extensions.Hosting;

using Serilog;

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
		Log.Logger.Debug("Creating host application builder for {HostType} with {AssemblyCount} assemblies", typeof(THost).Name, assemblies.Length);
		THostApplicationBuilder builder = create(args);
		Log.Logger.Debug("Running automatic dependency injection for {BuilderType}", typeof(THostApplicationBuilder).Name);
		await builder.AutomaticDependencyInjection(cancellationToken, assemblies);
		Log.Logger.Debug("Automatic dependency injection completed for {BuilderType}", typeof(THostApplicationBuilder).Name);
		Log.Logger.Debug("Running host builder configuration for {BuilderType}", typeof(THostApplicationBuilder).Name);
		configureBuilder?.Invoke(builder);

		Log.Logger.Debug("Building host {HostType}", typeof(THost).Name);
		THost application = build(builder);
		Log.Logger.Debug("Running automatic host setup for {HostType}", typeof(THost).Name);
		await application.AutomaticHostSetup<THost>(cancellationToken, assemblies);
		Log.Logger.Debug("Automatic host setup completed for {HostType}", typeof(THost).Name);
		Log.Logger.Debug("Running host configuration for {HostType}", typeof(THost).Name);
		configureHost?.Invoke(application);

		Log.Logger.Debug("Host {HostType} built", typeof(THost).Name);
		return application;
	}
}
