using System.ComponentModel.DataAnnotations;
using System.Reflection;

using JasperFx;

using Microsoft.Extensions.Hosting;

using Serilog;

namespace AEL.Core;

public static class HostRunner
{
	public static async Task<int> Run<THost, THostApplicationBuilder>(
		string[] args,
		bool disableJasper,
		Func<string[], THostApplicationBuilder> create,
		Func<THostApplicationBuilder, THost> build,
		params Assembly[] assemblies)
		where THost : IHost
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		using CancellationTokenSource cts = new();
		using Startup startup = new();
		try
		{
			Log.Logger.Information("Starting host runner for {HostType} with {AssemblyCount} assemblies", typeof(THost).Name, assemblies.Length);
			using THost host = await HostBuilder.Build(args, create, null, build, null, cts.Token, assemblies);
			Log.Logger.Debug("Host runner built {HostType}", typeof(THost).Name);
			if (disableJasper)
			{
				Log.Logger.Debug("Running host {HostType} without JasperFx commands", typeof(THost).Name);
				await host.RunAsync(token: cts.Token);
				Log.Logger.Debug("Host {HostType} stopped", typeof(THost).Name);
				return 0;
			}

			Log.Logger.Debug("Running JasperFx commands for host {HostType}", typeof(THost).Name);
			return await host.RunJasperFxCommands(args);
		}
		catch (OperationCanceledException)
		{
			// Ignore
			return 0;
		}
		catch (ValidationException ex)
		{
			Log.Logger.Fatal(ex.Message);
#if DEBUG
			throw; // For unit test
#else
			return -1;
#endif
		}
		catch (Exception ex)
		{
			Log.Logger.Fatal(ex, "Host terminated unexpectedly");
			return -1;
		}
		finally
		{
			Log.Logger.Information("Host runner stopping for {HostType}", typeof(THost).Name);
			await cts.CancelAsync();
		}
	}
}
