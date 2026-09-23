using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using FluentValidation;

using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace AEL.Core;

public sealed class Startup : DisposableBase
{
	private readonly ILogger _previousLogger;
	private readonly object? _previousRegexTimeout;
	private readonly Encoding _previousOutputEncoding;
	private readonly Encoding _previousInputEncoding;
	private readonly CultureInfo? _previousCulture;
	private readonly CultureInfo? _previousUiCulture;
	private readonly Func<Type, MemberInfo?, LambdaExpression?, string>? _previousDisplayNameResolver;
	private readonly UnhandledExceptionEventHandler _unhandledExceptionHandler;
	private readonly EventHandler<UnobservedTaskExceptionEventArgs> _unobservedTaskExceptionHandler;

	public Startup()
	{
		_previousLogger = Log.Logger;
		_previousRegexTimeout = AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT");
		_previousOutputEncoding = Console.OutputEncoding;
		_previousInputEncoding = Console.InputEncoding;
		_previousCulture = CultureInfo.DefaultThreadCurrentCulture;
		_previousUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
		_previousDisplayNameResolver = ValidatorOptions.Global.DisplayNameResolver;

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.Enrich.WithExceptionDetails()
			.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
			.WriteTo.OpenTelemetry(_ => { })
			.CreateBootstrapLogger();

		AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2.0));

		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		_unhandledExceptionHandler = OnUnhandledException;
		_unobservedTaskExceptionHandler = OnUnobservedTaskException;
		AppDomain.CurrentDomain.UnhandledException += _unhandledExceptionHandler;
		TaskScheduler.UnobservedTaskException += _unobservedTaskExceptionHandler;

		// Report JSON property names in validation errors, as API clients see those rather than C# names.
		ValidatorOptions.Global.DisplayNameResolver = (type, member, _) =>
		{
			if (member is null)
			{
				return type.Name;
			}

			string? jsonPropertyName = member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
			return string.IsNullOrEmpty(jsonPropertyName) ? member.Name : jsonPropertyName;
		};

		DisposableBag.Add(Restore);
	}

	private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception: {ExceptionObject}", e.ExceptionObject);
	}

	private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		Log.Logger.Error(e.Exception, "Background Task Error");
		e.SetObserved();
	}

	private void Restore()
	{
		AppDomain.CurrentDomain.UnhandledException -= _unhandledExceptionHandler;
		TaskScheduler.UnobservedTaskException -= _unobservedTaskExceptionHandler;

		Log.CloseAndFlush();
		Log.Logger = _previousLogger;

		AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", _previousRegexTimeout);
		CultureInfo.DefaultThreadCurrentCulture = _previousCulture;
		CultureInfo.DefaultThreadCurrentUICulture = _previousUiCulture;
		ValidatorOptions.Global.DisplayNameResolver = _previousDisplayNameResolver;

		try
		{
			Console.OutputEncoding = _previousOutputEncoding;
			Console.InputEncoding = _previousInputEncoding;
		}
		catch (IOException)
		{
			// Console encoding cannot be restored when the stream is redirected.
		}
	}
}
