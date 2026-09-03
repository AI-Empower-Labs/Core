using System.Collections.Frozen;

using FastEnumUtility;

namespace AEL.Core.Extensions;

public static class EnumExtensions
{
	extension(string source)
	{
		public bool TryParseEnumValue<T>(out T result)
			where T : struct, Enum
		{
			if (FastEnum.TryParse(source, true, out result))
			{
				return true;
			}

			FrozenDictionary<string, T> enumMemberLookup = GetEnumMemberLookup<T>();
			return enumMemberLookup.TryGetValue(source, out result);
		}
	}

	extension(string? source)
	{
		public T ParseEnumValue<T>(T defaultValue)
			where T : struct, Enum
		{
			if (string.IsNullOrEmpty(source))
			{
				return defaultValue;
			}

			return TryParseEnumValue(source, out T result) ? result : defaultValue;
		}
	}

	public static string[] GetEnumNames<T>()
		where T : struct, Enum =>
	[
		.. FastEnum
			.GetMembers<T>()
			.Select(member => member.EnumMemberAttribute?.Value ?? member.Name)
	];

	public static string GetEnumName<T>(this T value)
		where T : struct, Enum
	{
		Member<T> member = FastEnum.GetMember(value)!;
		return member.EnumMemberAttribute?.Value ?? member.Name;
	}

	public static string[] GetEnumNames<T>(T[] allowedValues)
		where T : struct, Enum =>
	[
		.. allowedValues
			.Select(static @enum =>
			{
				Member<T>? member = FastEnum.GetMember(@enum);
				return member?.EnumMemberAttribute?.Value ?? member?.Name ?? @enum.ToString();
			})
	];

	private static class EnumMemberCache<T> where T : struct, Enum
	{
		public static readonly FrozenDictionary<string, T> Value = FastEnum
			.GetMembers<T>()
			.ToFrozenDictionary(
				member => member.EnumMemberAttribute?.Value ?? member.Name,
				member => member.Value,
				StringComparer.OrdinalIgnoreCase);
	}

	private static FrozenDictionary<string, T> GetEnumMemberLookup<T>()
		where T : struct, Enum => EnumMemberCache<T>.Value;
}
