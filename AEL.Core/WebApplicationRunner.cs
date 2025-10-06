using System.ComponentModel.DataAnnotations;
using System.Reflection;

using JasperFx;

using Microsoft.AspNetCore.Builder;

using Serilog;

namespace AEL.Core;

public static class WebApplicationRunner
{
	public static Task<int> Run(string[] args, bool disableJasper = false)
	{
		return Run(args, disableJasper, null, null, [Assembly.GetEntryAssembly()!]);
	}

	public static Task<int> Run(
		string[] args,
		params Assembly[] assemblies)
	{
		return Run(args, false, null, null, assemblies);
	}

	public static Task<int> Run(
		string[] args,
		bool disableJasper = false,
		params Assembly[] assemblies)
	{
		return Run(args, disableJasper, null, null, assemblies);
	}

	public static async Task<int> Run(
		string[] args,
		bool disableJasper = false,
		Action<WebApplicationBuilder>? configureBuilder = null,
		Action<WebApplication>? configureApplication = null,
		params Assembly[] assemblies)
	{
		using CancellationTokenSource cts = new();
		using Startup startup = new();
		try
		{
			await using WebApplication application = await WebAppBuilder
				.Build(args, configureBuilder, configureApplication, cts.Token, assemblies);
			if (disableJasper)
			{
				await application.RunAsync();
				return 0;
			}

			return await application.RunJasperFxCommands(args);
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
