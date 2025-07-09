using AEL.Core.Extensions;

using FluentValidation.Results;

// ReSharper disable once CheckNamespace
namespace FluentValidation;

public static class FluentValidationExtensions
{
	public static IDictionary<string, string[]> ToDictionary(this IEnumerable<ValidationFailure> validationFailures)
	{
		return validationFailures
			.GroupBy(validationFailure => validationFailure.PropertyName)
			.ToDictionary(
				g => g.Key,
				g => g.Select(static failure => failure.ErrorMessage).ToArray()
			);
	}

	public static IRuleBuilderOptions<T, Uri> MustBeAbsoluteUri<T>(this IRuleBuilder<T, Uri> ruleBuilder)
	{
		return ruleBuilder
			.NotNull()
			.Must(uri => uri?.IsAbsoluteUri != null)
			.WithMessage("Must be an absolute uri");
	}

	public static IRuleBuilderOptions<T, string?> MustBeOneOf<T>(this IRuleBuilder<T, string?> ruleBuilder, params string[] allowedValues)
	{
		return ruleBuilder.MustBeOneOf(StringComparer.OrdinalIgnoreCase, allowedValues);
	}

	public static IRuleBuilderOptions<T, string?> MustBeOneOf<T>(this IRuleBuilder<T, string?> ruleBuilder, StringComparer stringComparer, params string[] allowedValues)
	{
		return ruleBuilder
			.Must(s => s is not null && allowedValues.Any(oneOf => stringComparer.Equals(oneOf, s)))
			.WithMessage($"Must be one of {string.Join(',', allowedValues)}");
	}

	public static IRuleBuilderOptions<T, TOneOf?> MustBeOneOf<T, TOneOf>(this IRuleBuilder<T, TOneOf?> ruleBuilder,
		params TOneOf[] allowedValues)
		where TOneOf : struct, Enum
	{
		return ruleBuilder
			.Must(s => s is not null && allowedValues.Any(oneOf => oneOf.Equals(s)))
			.WithMessage($"Must be one of {string.Join(',', EnumExtensions.GetEnumNames(allowedValues))}");
	}

	public static IRuleBuilderOptions<T, string> DefaultStringValidation<T>(this IRuleBuilder<T, string> ruleBuilder,
		int minLength = 1,
		int maxLength = 255)
	{
		return ruleBuilder
			.NotEmpty()
			.WithMessage("Cannot be empty")
			.MinimumLength(minLength)
			.WithMessage($"Must have a minimum length of {minLength}")
			.MaximumLength(maxLength)
			.WithMessage($"Cannot exceed length of {maxLength}");
	}
}
