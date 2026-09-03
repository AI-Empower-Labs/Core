using System.Reflection;

using AEL.Core.Interfaces;
using AEL.Core.Registration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AEL.Core.Tests.Registration;

public sealed class AutomaticRegistrationTests
{
	public interface ITestTransient : ITransientService;
	public sealed class TestTransient : ITestTransient;

	public interface ITestScoped : IScopedService;
	public sealed class TestScoped : ITestScoped;

	public interface ITestSingleton : ISingletonService;
	public sealed class TestSingleton : ITestSingleton;

	public sealed class CustomSyncDiRegistration : IDependencyInjectionRegistration<HostApplicationBuilder>
	{
		public static bool WasRegistered { get; private set; }

		public static void Register(HostApplicationBuilder builder)
		{
			WasRegistered = true;
			builder.Services.AddKeyedSingleton("custom-sync", "sync-ok");
		}
	}

	public sealed class CustomAsyncDiRegistration : IDependencyInjectionRegistrationAsync<HostApplicationBuilder>
	{
		public static bool WasRegistered { get; private set; }

		public static async ValueTask Register(HostApplicationBuilder builder, CancellationToken cancellationToken = default)
		{
			await Task.Yield();
			WasRegistered = true;
			builder.Services.AddKeyedSingleton("custom-async", "async-ok");
		}
	}

	[ServiceProviderRegistrationOrder(1)]
	public sealed class OrderedHostSetupOne : IHostSetup<IHost>
	{
		public static List<string> ExecutionLog { get; } = [];

		public static void Setup(IHost host)
		{
			ExecutionLog.Add("first");
		}
	}

	[ServiceProviderRegistrationOrder(2)]
	public sealed class OrderedHostSetupTwo : IHostSetupAsync<IHost>
	{
		public static async ValueTask Setup(IHost host, CancellationToken cancellationToken = default)
		{
			await Task.Yield();
			OrderedHostSetupOne.ExecutionLog.Add("second");
		}
	}

	private sealed class NonSingletonHostedService : IHostedService
	{
		public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}

	[Fact]
	public async Task AutomaticDependencyInjection_RegistersServicesAndCustomHandlers()
	{
		HostApplicationBuilder builder = new();
		Assembly testAssembly = typeof(AutomaticRegistrationTests).Assembly;

		await builder.AutomaticDependencyInjection(TestContext.Current.CancellationToken, testAssembly);
		using IHost host = builder.Build();

		Assert.True(CustomSyncDiRegistration.WasRegistered);
		Assert.True(CustomAsyncDiRegistration.WasRegistered);

		using IServiceScope scope = host.Services.CreateScope();
		Assert.NotNull(scope.ServiceProvider.GetService<ITestTransient>());
		Assert.NotNull(scope.ServiceProvider.GetService<ITestScoped>());
		Assert.NotNull(scope.ServiceProvider.GetService<ITestSingleton>());

		Assert.Equal("sync-ok", host.Services.GetKeyedService<string>("custom-sync"));
		Assert.Equal("async-ok", host.Services.GetKeyedService<string>("custom-async"));
	}

	[Fact]
	public async Task AutomaticHostSetup_RespectsRegistrationOrder()
	{
		HostApplicationBuilder builder = new();
		using IHost host = builder.Build();
		Assembly testAssembly = typeof(AutomaticRegistrationTests).Assembly;

		OrderedHostSetupOne.ExecutionLog.Clear();
		await host.AutomaticHostSetup<IHost>(TestContext.Current.CancellationToken, testAssembly);

		Assert.Equal(["first", "second"], OrderedHostSetupOne.ExecutionLog);
	}

	[Fact]
	public void RegisterType_HostedServiceAsTransient_ThrowsInvalidOperationException()
	{
		ServiceCollection services = [];
		Assert.Throws<InvalidOperationException>(() =>
			services.RegisterType(typeof(NonSingletonHostedService), ServiceLifetime.Transient));
	}
}
