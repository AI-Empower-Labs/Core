using System.Collections.Frozen;

using FastEnumUtility;

namespace AEL.Core.Extensions;

public static class EnumExtensions
{
	public static bool TryParseEnumValue<T>(this string source, out T result)
		where T : struct, Enum
	{
		if (FastEnum.TryParse(source, true, out result))
		{
			return true;
		}

		FrozenDictionary<string, T> enumMemberLookup = GetEnumMemberLookup<T>();
		return enumMemberLookup.TryGetValue(source, out result);
	}

	public static T ParseEnumValue<T>(this string? source, T defaultValue)
		where T : struct, Enum
	{
		if (string.IsNullOrEmpty(source))
		{
			return defaultValue;
		}

		return TryParseEnumValue(source, out T result) ? result : defaultValue;
	}

	public static string[] GetEnumNames<T>()
		where T : struct, Enum
	{
		return FastEnum
			.GetMembers<T>()
			.Select(member => member.EnumMemberAttribute?.Value ?? member.Name)
			.ToArray();
	}

	public static string GetEnumName<T>(this T value)
		where T : struct, Enum
	{
		Member<T> member = FastEnum.GetMember(value)!;
		return member.EnumMemberAttribute?.Value ?? member.Name;
	}

	public static string[] GetEnumNames<T>(T[] allowedValues)
		where T : struct, Enum
	{
		return allowedValues
			.Select(static @enum =>
			{
				Member<T>? member = FastEnum.GetMember(@enum);
				return member?.EnumMemberAttribute?.Value ?? member?.Name ?? @enum.ToString();
			})
			.ToArray();
	}

	private static readonly IDictionary<Type, object> s_enumToValueMap = new Dictionary<Type, object>();
	private static FrozenDictionary<string, T> GetEnumMemberLookup<T>()
		where T : struct, Enum
	{
		if (s_enumToValueMap.TryGetValue(typeof(T), out object? g)
			&& g is FrozenDictionary<string, T> enumMemberLookup)
		{
			return enumMemberLookup;
		}

		s_enumToValueMap[typeof(T)] = enumMemberLookup = FastEnum
			.GetMembers<T>()
			.ToFrozenDictionary(member => member.EnumMemberAttribute?.Value ?? member.Name, member => member.Value);
		return enumMemberLookup;
	}
}
