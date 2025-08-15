using FluentValidation;
using FluentValidation.Results;

namespace AEL.Core.Tests.Extensions;

public class FluentValidationExtensionsTests
{
	public class Model
	{
		public Uri? Address { get; set; }
		public string? Mode { get; set; }
		public TestEnum? Choice { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public enum TestEnum
	{
		A,
		B
	}

	public class ModelValidator : AbstractValidator<Model>
	{
		public ModelValidator()
		{
			RuleFor(model => model.Address!).MustBeAbsoluteUri();
			RuleFor(model => model.Mode).MustBeOneOf("on", "off");
			RuleFor(model => model.Choice).MustBeOneOf(TestEnum.A, TestEnum.B);
			RuleFor(model => model.Name).DefaultStringValidation(2, 5);
		}
	}

	[Fact]
	public void ToDictionary_GroupsByProperty()
	{
		ValidationFailure[] failures =
		[
			new("A", "err1"),
			new("A", "err2"),
			new("B", "e")
		];

		IDictionary<string, string[]> dict = failures.ToDictionary();
		Assert.Equal(new[] { "err1", "err2" }, dict["A"]);
		Assert.Equal(new[] { "e" }, dict["B"]);
	}

	[Fact]
	public void CustomRules_Work()
	{
		ModelValidator v = new();
		Model invalid = new()
			{ Address = new Uri("/rel", UriKind.Relative), Mode = "maybe", Choice = null, Name = string.Empty };
		Assert.False(v.Validate(invalid).IsValid);

		Model valid = new()
			{ Address = new Uri("https://example.com"), Mode = "ON", Choice = TestEnum.A, Name = "Ab" };
		Assert.True(v.Validate(valid).IsValid);

		// Name length
		Model tooLong = new()
			{ Address = new Uri("https://x"), Mode = "off", Choice = TestEnum.B, Name = "123456" };
		ValidationResult? res2 = v.Validate(tooLong);
		Assert.Contains(res2.Errors, e => e.PropertyName == nameof(Model.Name));
	}
}
