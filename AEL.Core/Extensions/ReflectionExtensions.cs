// ReSharper disable once CheckNamespace

namespace System;

public static class ReflectionExtensions
{
	public static bool IsBasedOn(this Type type, Type otherType)
	{
		return otherType.IsGenericTypeDefinition
			? type.IsAssignableToGenericTypeDefinition(otherType)
			: otherType.IsAssignableFrom(type);
	}

	private static bool IsAssignableToGenericTypeDefinition(this Type type, Type genericType)
	{
		foreach (Type interfaceType in type.GetInterfaces())
		{
			if (!interfaceType.IsGenericType) continue;
			Type genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
			if (genericTypeDefinition == genericType)
			{
				return true;
			}
		}

		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == genericType)
			{
				return true;
			}
		}

		Type? baseType = type.BaseType;
		if (baseType is null)
		{
			return false;
		}

		return baseType.IsAssignableToGenericTypeDefinition(genericType);
	}
}
