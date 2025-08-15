namespace AEL.Core.Tests.Extensions;

public class ReflectionExtensionsTests
{
	public interface IGeneric<T>;

	public class Base : IGeneric<int>;

	public class Derived : Base;

	[Fact]
	public void IsBasedOn_WorksForGenericInterfaces()
	{
		Assert.True(typeof(Derived).IsBasedOn(typeof(IGeneric<>)));
		Assert.False(typeof(string).IsBasedOn(typeof(IGeneric<>)));
	}

	[Fact]
	public void IsBasedOn_WorksForNonGeneric()
	{
		Assert.True(typeof(Derived).IsBasedOn(typeof(Base)));
		Assert.True(typeof(Derived).IsBasedOn(typeof(object)));
		Assert.False(typeof(Base).IsBasedOn(typeof(Derived)));
	}
}
