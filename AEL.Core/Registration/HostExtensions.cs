using System.Reflection;

using AEL.Core.Registration;

using Serilog;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostExtensions
{
	public static async ValueTask AutomaticHostSetup<THost>(this IHost host, CancellationToken cancellationToken, params Assembly[] assemblies) where THost : IHost
	{
		Log.Logger.Debug("Starting automatic host setup for {HostType} with {AssemblyCount} assemblies", typeof(THost).Name, assemblies.Length);
		foreach (Type type in TypeResolverHelper.GetTypes(
			selector =>
			{
				selector.AddClasses(filter => filter.AssignableTo(typeof(IHostSetup<THost>)), false);
				selector.AddClasses(filter => filter.AssignableTo(typeof(IHostSetupAsync<THost>)), false);
			},
			assemblies))
		{
			if (type.IsBasedOn(typeof(IHostSetup<THost>)))
			{
				Log.Logger.Debug("Running host setup {Type}", type.FullName);
				MethodInfo? methodInfo = type.GetMethod(
					nameof(IHostSetup<>.Setup),
					BindingFlags.Public | BindingFlags.Static,
					[typeof(THost)]);
				if (methodInfo is null || methodInfo.ReturnType != typeof(void))
				{
					throw new InvalidOperationException(
						$"Type {type.FullName} implements {nameof(IHostSetup<>)}<{typeof(THost).Name}> " +
						$"but does not declare: public static void {nameof(IHostSetup<>.Setup)}({typeof(THost).Name} host).");
				}

				methodInfo.Invoke(null, [host]);
				Log.Logger.Debug("Host setup {Type} completed", type.FullName);
			}
			else if (type.IsBasedOn(typeof(IHostSetupAsync<THost>)))
			{
				Log.Logger.Debug("Running async host setup {Type}", type.FullName);
				MethodInfo? methodInfo = type.GetMethod(
					nameof(IHostSetupAsync<>.Setup),
					BindingFlags.Public | BindingFlags.Static,
					[typeof(THost), typeof(CancellationToken)])
					?? type.GetMethod(
						nameof(IHostSetupAsync<>.Setup),
						BindingFlags.Public | BindingFlags.Static,
						[typeof(THost)]);
				if (methodInfo is null)
				{
					throw new InvalidOperationException(
						$"Type {type.FullName} implements {nameof(IHostSetupAsync<>)}<{typeof(THost).Name}> " +
						$"but does not declare: public static ValueTask {nameof(IHostSetupAsync<>.Setup)}({typeof(THost).Name} host, CancellationToken cancellationToken).");
				}

				object?[] parameters = methodInfo.GetParameters().Length == 2
					? [host, cancellationToken]
					: [host];
				object? valueTaskObject = methodInfo.Invoke(null, parameters);
				switch (valueTaskObject)
				{
					case ValueTask valueTask:
						await valueTask;
						break;
					case Task task:
						await task;
						break;
					default:
						throw new InvalidOperationException(
							$"Type {type.FullName} implements {nameof(IHostSetupAsync<>)}<{typeof(THost).Name}> " +
							$"but {nameof(IHostSetupAsync<>.Setup)} did not return a ValueTask or Task.");
				}

				Log.Logger.Debug("Async host setup {Type} completed", type.FullName);
			}
		}

		Log.Logger.Debug("Automatic host setup completed for {HostType}", typeof(THost).Name);
	}
}
