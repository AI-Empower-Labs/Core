using System.Globalization;

using FluentValidation;

namespace AEL.Core.Tests;

public sealed class StartupTests
{
	[Fact]
	public void Dispose_RestoresProcessState_AndUnsubscribesHandlers()
	{
		CultureInfo? previousCulture = CultureInfo.DefaultThreadCurrentCulture;
		CultureInfo? previousUICulture = CultureInfo.DefaultThreadCurrentUICulture;
		Func<Type, System.Reflection.MemberInfo?, System.Linq.Expressions.LambdaExpression?, string>? previousResolver =
			ValidatorOptions.Global.DisplayNameResolver;
		CultureInfo danish = CultureInfo.GetCultureInfo("da-DK");

		try
		{
			CultureInfo.DefaultThreadCurrentCulture = danish;
			CultureInfo.DefaultThreadCurrentUICulture = danish;

			using (Startup startup = new())
			{
				Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.DefaultThreadCurrentCulture);
				Assert.Equal(CultureInfo.InvariantCulture, CultureInfo.DefaultThreadCurrentUICulture);
				Assert.NotSame(previousResolver, ValidatorOptions.Global.DisplayNameResolver);
				Assert.False(startup.IsDisposed);
			}

			Assert.Equal(danish, CultureInfo.DefaultThreadCurrentCulture);
			Assert.Equal(danish, CultureInfo.DefaultThreadCurrentUICulture);
			Assert.Same(previousResolver, ValidatorOptions.Global.DisplayNameResolver);
		}
		finally
		{
			CultureInfo.DefaultThreadCurrentCulture = previousCulture;
			CultureInfo.DefaultThreadCurrentUICulture = previousUICulture;
			ValidatorOptions.Global.DisplayNameResolver = previousResolver;
		}
	}
}
