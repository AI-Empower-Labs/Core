using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AEL.Core.Serialization;

public sealed class JsonStringEnumConverterWithEnumMemberAttrSupport<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>() :
	JsonStringEnumConverter<TEnum>(namingPolicy: ResolveNamingPolicy())
	where TEnum : struct, Enum
{
	private static EnumMemberNamingPolicy? ResolveNamingPolicy()
	{
		Dictionary<string, string?> map = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
			.Select(static f => (f.Name, AttributeName: f.GetCustomAttribute<EnumMemberAttribute>()?.Value))
			.Where(pair => pair.AttributeName is not null)
			.ToDictionary();

		return map.Count > 0 ? new EnumMemberNamingPolicy(map!) : null;
	}

	private sealed class EnumMemberNamingPolicy(IReadOnlyDictionary<string, string> map) : JsonNamingPolicy
	{
		public override string ConvertName(string name) => map.GetValueOrDefault(name, name);
	}
}
