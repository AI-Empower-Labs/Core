# AEL.Core

Shared .NET libraries for [AI Empower Labs](https://github.com/AI-Empower-Labs) applications.

| Package | Description |
|---------|-------------|
| **AEL.Core** | Host bootstrap, convention-based DI, background services, extensions, and utilities |

## Features

### AEL.Core

- **Host runners**: One-line startup for web and console apps with Serilog, OpenTelemetry, and JasperFx
- **Convention-based DI**: Register services via marker interfaces (`IScopedService`, `ITransientService`, `ISingletonService`)
- **Background services**: Async hosted services, cron scheduling, and channel-based batch processing
- **Extensions**: Extension methods for common .NET types (strings, tasks, channels, async enumerables, etc.)
- **Disposables**: Composable disposable patterns (`DisposableBag`, `AsyncDisposableBase`, `AsyncCompletionScope`, etc.)
- **Serialization**: JSON converters and enum handling with `[EnumMember]` support
- **Utilities**: Nanoid generation, continuous hashing, temp files, progress streams, file name sanitization

## Installation

### Submodule

```
git submodule add https://github.com/AI-Empower-Labs/Core
```

Add a project reference to `AEL.Core`.

### NuGet

Packages are published to GitHub Packages:

```
dotnet add package AEL.Core --source https://nuget.pkg.github.com/AI-Empower-Labs/index.json
```

## Quick Start

### Web application

Starts a web application and wires up DI, logging, and host setup automatically:

```csharp
using System.Reflection;
using AEL.Core;

return await WebApplicationRunner.Run(args, Assembly.GetEntryAssembly()!);
```

### Console application

```csharp
using System.Reflection;
using AEL.Core;

return await ConsoleApplicationRunner.Run(args, Assembly.GetEntryAssembly()!);
```

### Convention-based Dependency Injection

Implement lifetime markers to have services discovered and registered automatically across assemblies:

```csharp
using AEL.Core.Interfaces;

public interface IMyService
{
    void DoWork();
}

public class MyService : IMyService, IScopedService
{
    public void DoWork() { }
}
```

## Project Structure

```
Core/
├── AEL.Core/
│   ├── Disposables/             # Resource management utilities
│   ├── Extensions/              # Extension methods for common types
│   ├── Interfaces/              # DI markers, host setup contracts
│   ├── Registration/            # Automatic DI and host setup
│   ├── Serialization/           # JSON converters
│   ├── Stream/                  # Progress stream helpers
│   ├── AsyncBackgroundService.cs
│   ├── AsyncBatchProcessor.cs   # Channel-based batch processing
│   ├── CronExecutionAsyncBackgroundService.cs
│   ├── ConsoleApplicationRunner.cs
│   ├── ContinuousHash.cs
│   ├── FileNameHelper.cs
│   ├── HostBuilder.cs / HostRunner.cs
│   ├── LoggingRegistration.cs
│   ├── NanoIdGenerator.cs       # Nanoid class
│   ├── OpenTelemetryRegistration.cs
│   ├── Startup.cs
│   ├── TempFile.cs
│   ├── TestRunner.cs / HostTestRunner.cs
│   └── WebApplicationRunner.cs
└── AEL.Core.Tests/
```

## Components

### Background Services

- `AsyncBackgroundService`: Base class for long-running async hosted services
- `CronExecutionAsyncBackgroundService`: Cron-scheduled periodic execution
- `AsyncBatchProcessor` / `AsyncBatchProcessor<TIn, TOut>`: Channel-based batch processing

### Host & DI

- `HostRunner` / `WebApplicationRunner` / `ConsoleApplicationRunner`: Application entry points
- `HostBuilder`: Builds hosts with automatic DI registration and host setup
- `TestRunner` / `HostTestRunner`: Starts a host for integration testing (optionally without hosted services)
- Marker interfaces drive lifetime registration; `IDependencyInjectionRegistration*` types add custom registration

### Utilities

- **Nanoid** (`NanoIdGenerator.cs`): Generate unique, URL-safe identifiers
- **ContinuousHash** (Obsolete): Use `System.Security.Cryptography.IncrementalHash` instead
- **TempFile**: Temporary file creation and management
- **ProgressStream**: Stream wrapper tracking read/write byte progress
- **FileNameHelper**: Sanitize file names for the local filesystem
- **OpenTelemetryRegistration** / **LoggingRegistration**: Observability and structured logging setup

## Requirements

- .NET 10 or later

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/AI-Empower-Labs/Core/issues) on GitHub.
