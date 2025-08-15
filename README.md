# AEL.Core

A comprehensive .NET Core library providing essential utilities, extensions, and services for building robust applications.

## Features

- **Extensions**: Useful extension methods for common .NET types
- **Background Services**: Asynchronous background service implementations
- **Serialization**: Data serialization utilities
- **Disposables**: Resource management utilities
- **Temp File Management**: Temporary file handling
- **Web Application Utilities**: Web application builder and runner
- **ID Generation**: NanoId generator for unique identifiers
- **Hashing**: Continuous hash utilities

## Installation

Add as a submodule and add reference to project

```
git submodule add https://github.com/AI-Empower-Labs/Core
```


## Quick Start

Starts Web Application and wires up everything automatically

```csharp
using System.Reflection;
using AEL.Core;

return await WebApplicationRunner.Run(args, Assembly.GetEntryAssembly()!);
```


## Project Structure

```
AEL.Core/
├── Extensions/                 # Extension methods for common types
├── Interfaces/                 # Interface definitions
├── Disposables/               # Resource management utilities
├── Registration/              # Service registration utilities
├── Serialization/             # Serialization utilities
├── AsyncBackgroundService.cs  # Base async background service
├── PeriodicExecutionAsyncBackgroundService.cs  # Periodic execution service
├── ContinuesHash.cs          # Continuous hashing utility
├── NanoIdGenerator.cs        # Unique ID generation
├── TempFile.cs               # Temporary file management
├── WebApplicationBuilder.cs  # Web application builder
├── WebApplicationRunner.cs   # Web application runner
└── Startup.cs                # Application startup configuration
```


## Components

### Background Services

The library provides base classes for implementing background services:

- `AsyncBackgroundService`: Base class for asynchronous background services
- `PeriodicExecutionAsyncBackgroundService`: Service for periodic task execution

### Utilities

- **NanoIdGenerator**: Generate unique, URL-safe identifiers
- **ContinuesHash**: Continuous hashing functionality
- **TempFile**: Temporary file creation and management

### Web Application Support

- **WebApplicationBuilder**: Enhanced web application builder
- **WebApplicationRunner**: Web application execution utilities

## Requirements

- .NET 9 or later

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
