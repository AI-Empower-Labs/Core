using AEL.Core.Extensions;

using FluentValidation.Results;

// ReSharper disable once CheckNamespace
namespace FluentValidation;

public static class FluentValidationExtensions
{
	extension(IEnumerable<ValidationFailure> validationFailures)
	{
		public IDictionary<string, string[]> ToDictionary()
		{
			return validationFailures
				.GroupBy(validationFailure => validationFailure.PropertyName)
				.ToDictionary(
					g => g.Key,
					g => g.Select(static failure => failure.ErrorMessage).ToArray()
				);
		}
	}

	extension<T>(IRuleBuilder<T, Uri> ruleBuilder)
	{
		public IRuleBuilderOptions<T, Uri> MustBeAbsoluteUri()
		{
			return ruleBuilder
				.NotNull()
				.Must(uri => uri?.IsAbsoluteUri is not null)
				.WithMessage("Must be an absolute uri");
		}
	}

	extension<T>(IRuleBuilder<T, string?> ruleBuilder)
	{
		public IRuleBuilderOptions<T, string?> MustBeOneOf(params string[] allowedValues)
		{
			return ruleBuilder.MustBeOneOf(StringComparer.OrdinalIgnoreCase, allowedValues);
		}
		public IRuleBuilderOptions<T, string?> MustBeOneOf(StringComparer stringComparer, params string[] allowedValues)
		{
			return ruleBuilder
				.Must(s => s is not null && allowedValues.Any(oneOf => stringComparer.Equals(oneOf, s)))
				.WithMessage($"Must be one of {string.Join(',', allowedValues)}");
		}
	}

	extension<T, TOneOf>(IRuleBuilder<T, TOneOf?> ruleBuilder) where TOneOf : struct, Enum
	{
		public IRuleBuilderOptions<T, TOneOf?> MustBeOneOf(params TOneOf[] allowedValues)
		{
			return ruleBuilder
				.Must(s => s is not null && allowedValues.Any(oneOf => oneOf.Equals(s)))
				.WithMessage($"Must be one of {string.Join(',', EnumExtensions.GetEnumNames(allowedValues))}");
		}
	}

	extension<T>(IRuleBuilder<T, string> ruleBuilder)
	{
		public IRuleBuilderOptions<T, string> DefaultStringValidation(int minLength = 1,
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
}
