namespace AEL.Core.Registration;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceProviderRegistrationOrderAttribute(int order) : Attribute
{
	public int Order { get; } = order;
}
