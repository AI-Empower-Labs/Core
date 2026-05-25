using System.Reflection;

using AEL.Core.Registration;

using Serilog;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostExtensions
{
	public static async ValueTask AutomaticHostSetup<THost>(this IHost host, CancellationToken cancellationToken, params Assembly[] assemblies) where THost : IHost
	{
		Log.Logger.Debug("Starting automatic host setup for {HostType} with {AssemblyCount} assemblies", typeof(THost).Name, assemblies.Length);
		foreach (Type type in TypeResolverHelper.GetTypes(
			selector =>
			{
				selector.AddClasses(filter => filter.AssignableTo(typeof(IHostSetup<THost>)), false);
				selector.AddClasses(filter => filter.AssignableTo(typeof(IHostSetupAsync<THost>)), false);
			},
			assemblies))
		{
			if (type.IsBasedOn(typeof(IHostSetup<THost>)))
			{
				Log.Logger.Debug("Running host setup {Type}", type.FullName);
				MethodInfo? methodInfo = type.GetMethod(nameof(IHostSetup<>.Setup));
				methodInfo?.Invoke(null, [host]);
				Log.Logger.Debug("Host setup {Type} completed", type.FullName);
			}
			else if (type.IsBasedOn(typeof(IHostSetupAsync<THost>)))
			{
				Log.Logger.Debug("Running async host setup {Type}", type.FullName);
				MethodInfo? methodInfo = type.GetMethod(nameof(IHostSetupAsync<>.Setup));
				object? valueTaskObject = methodInfo?.Invoke(null, [host, cancellationToken]);
				if (valueTaskObject is ValueTask valueTask)
				{
					await valueTask;
				}

				Log.Logger.Debug("Async host setup {Type} completed", type.FullName);
			}
		}

		Log.Logger.Debug("Automatic host setup completed for {HostType}", typeof(THost).Name);
	}
}
