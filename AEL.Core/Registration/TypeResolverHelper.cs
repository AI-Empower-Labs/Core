using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Scrutor;

namespace AEL.Core.Registration;

public static class TypeResolverHelper
{
	public static IEnumerable<Type> GetClassTypes(params Assembly[] assemblies)
	{
		return GetTypes(selector => selector.AddClasses(false), assemblies);
	}

	public static IEnumerable<Type> GetTypes(Action<IImplementationTypeSelector> action, params Assembly[] assemblies)
	{
		ServiceCollection collection = [];
		collection.Scan(selector =>
		{
			IImplementationTypeSelector fromAssemblies = selector.FromAssemblies(assemblies);
			action(fromAssemblies);
		});
		return collection
			.Select(descriptor => descriptor.ServiceType)
			.OrderBy(type =>
			{
				ServiceProviderRegistrationOrderAttribute? attribute = type.GetCustomAttribute<ServiceProviderRegistrationOrderAttribute>();
				return attribute?.Order ?? int.MaxValue;
			});
	}
}
