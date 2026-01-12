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
			using THost host = await HostBuilder.Build(args, create, null, build, null, cts.Token, assemblies);
			if (disableJasper)
			{
				await host.RunAsync(token: cts.Token);
				return 0;
			}

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
			await cts.CancelAsync();
		}
	}
}
