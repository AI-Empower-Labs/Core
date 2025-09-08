using System.Reflection;

using AEL.Core.Registration;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationExtensions
{
	public static async ValueTask AutomaticWebApplicationSetup(this WebApplication webApplication, CancellationToken cancellationToken, params Assembly[] assemblies)
	{
		foreach (Type type in TypeResolverHelper.GetTypes(
			selector =>
			{
				selector.AddClasses(filter => filter.AssignableTo(typeof(IWebApplicationSetup)), false);
				selector.AddClasses(filter => filter.AssignableTo(typeof(IWebApplicationSetupAsync)), false);
			},
			assemblies))
		{
			if (type.IsBasedOn(typeof(IWebApplicationSetup)))
			{
				MethodInfo? methodInfo = type.GetMethod(nameof(IWebApplicationSetup.Setup));
				methodInfo?.Invoke(null, [webApplication]);
			}
			else if (type.IsBasedOn(typeof(IWebApplicationSetupAsync)))
			{
				MethodInfo? methodInfo = type.GetMethod(nameof(IWebApplicationSetupAsync.Setup));
				object? valueTaskObject = methodInfo?.Invoke(null, [webApplication, cancellationToken]);
				if (valueTaskObject is ValueTask valueTask)
				{
					await valueTask;
				}
			}
		}
	}
}
