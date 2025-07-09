using System.Reflection;

using AEL.Core.Registration;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationExtensions
{
	public static void AutomaticWebApplicationSetup(this WebApplication webApplication, params Assembly[] assemblies)
	{
		foreach (Type type in TypeResolverHelper.GetTypes(
			selector => selector
				.AddClasses(filter => filter
					.AssignableTo(typeof(IWebApplicationSetup)), false),
			assemblies))
		{
			MethodInfo? methodInfo = type.GetMethod(nameof(IWebApplicationSetup.Setup));
			methodInfo?.Invoke(null, [webApplication]);
		}
	}
}
