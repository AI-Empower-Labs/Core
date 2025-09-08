using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class WebAppBuilder
{
	public static Task<WebApplication> Build(
		string[] args,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
	{
		return Build(args, null, null, cancellationToken, assemblies);
	}

	public static async Task<WebApplication> Build(
		string[] args,
		Action<WebApplicationBuilder>? configureBuilder,
		Action<WebApplication>? configureApplication,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
		await builder.AutomaticDependencyInjection(cancellationToken, assemblies);
		configureBuilder?.Invoke(builder);

		WebApplication application = builder.Build();
		await application.AutomaticWebApplicationSetup(cancellationToken, assemblies);
		configureApplication?.Invoke(application);

		return application;
	}
}
