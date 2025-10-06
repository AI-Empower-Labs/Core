using System.Reflection;

using AEL.Core.Registration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostExtensions
{
	public static async ValueTask AutomaticHostSetup<THost>(this IHost host, CancellationToken cancellationToken, params Assembly[] assemblies) where THost : IHost
	{
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
				MethodInfo? methodInfo = type.GetMethod(nameof(IHostSetup<THost>.Setup));
				methodInfo?.Invoke(null, [host]);
			}
			else if (type.IsBasedOn(typeof(IHostSetupAsync<THost>)))
			{
				MethodInfo? methodInfo = type.GetMethod(nameof(IHostSetupAsync<>.Setup));
				object? valueTaskObject = methodInfo?.Invoke(null, [host, cancellationToken]);
				if (valueTaskObject is ValueTask valueTask)
				{
					await valueTask;
				}
			}
		}
	}
}
