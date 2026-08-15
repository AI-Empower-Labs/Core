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
	private readonly CultureInfo? _previousUICulture;
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
		_previousUICulture = CultureInfo.DefaultThreadCurrentUICulture;
		_previousDisplayNameResolver = ValidatorOptions.Global.DisplayNameResolver;

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.Enrich.WithExceptionDetails()
			.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
			.WriteTo.OpenTelemetry(_ => { })
			.CreateBootstrapLogger();

		// Set a 2-second timeout for all Regex operations globally
		AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2.0));

		// Allows the console to display symbols, emojis, and foreign characters
		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		_unhandledExceptionHandler = OnUnhandledException;
		_unobservedTaskExceptionHandler = OnUnobservedTaskException;
		AppDomain.CurrentDomain.UnhandledException += _unhandledExceptionHandler;
		TaskScheduler.UnobservedTaskException += _unobservedTaskExceptionHandler;

		// Configure FluentValidation to use JSON property names in validation error messages
		// This ensures that validation error messages display the JSON property name (if available)
		// rather than the C# property name, which is useful for API responses
		ValidatorOptions.Global.DisplayNameResolver = (type, member, _) =>
		{
			// If no member is provided (validating the type itself), return the type name
			if (member is null)
			{
				return type.Name;
			}

			// Check if the member has a JsonPropertyNameAttribute (from System.Text.Json)
			JsonPropertyNameAttribute? propertyNameAttribute = member.GetCustomAttribute<JsonPropertyNameAttribute>();
			string? jsonPropertyName = propertyNameAttribute?.Name;

			// Return the JSON property name if available, otherwise fall back to the member name
			return string.IsNullOrEmpty(jsonPropertyName) ? member.Name : jsonPropertyName;
		};

		DisposableBag.Add(Restore);
	}

	private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Logger.Fatal($"CRITICAL ERROR: {e.ExceptionObject}");
	}

	private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		Log.Logger.Error(e.Exception, "Background Task Error");
		e.SetObserved(); // Prevents the process from crashing in older .NET versions
	}

	private void Restore()
	{
		AppDomain.CurrentDomain.UnhandledException -= _unhandledExceptionHandler;
		TaskScheduler.UnobservedTaskException -= _unobservedTaskExceptionHandler;

		Log.CloseAndFlush();
		Log.Logger = _previousLogger;

		AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", _previousRegexTimeout);
		CultureInfo.DefaultThreadCurrentCulture = _previousCulture;
		CultureInfo.DefaultThreadCurrentUICulture = _previousUICulture;
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
