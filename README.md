[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Latest Release](https://img.shields.io/github/v/release/hmlendea/steamguard.totp)](https://github.com/hmlendea/steamguard.totp/releases/latest) [![Build Status](https://github.com/hmlendea/steamguard.totp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steamguard.totp/actions/workflows/dotnet.yml)

# SteamGuard.TOTP

A lightweight .NET library for generating Steam Guard authentication codes from a Steam shared secret (Base32 TOTP key).

## Features

- Generates 5-character Steam Guard codes.
- Uses the Steam-compatible character set: `23456789BCDFGHJKMNPQRTVWXY`.
- Small API surface with interface-based abstractions.
- Supports dependency injection for deterministic testing through `ITimeStepProvider`.

## Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/SteamGuard.TOTP)

### .NET CLI

```bash
dotnet add package SteamGuard.TOTP
```

### Package Manager Console

```powershell
Install-Package SteamGuard.TOTP
```

## Target Framework

The project currently targets `net10.0`.

## Quick Start

```csharp
using SteamGuard.TOTP;

ISteamGuard steamGuard = new SteamGuard();
string sharedSecret = "JBSWY3DPEHPK3PXP"; // Example Base32 secret

string code = steamGuard.GenerateAuthenticationCode(sharedSecret);
Console.WriteLine(code); // e.g. "D57RM"
```

## API Overview

### `ISteamGuard`

```csharp
public interface ISteamGuard
{
		string GenerateAuthenticationCode(string totpKey);
}
```

### `ITimeStepProvider`

```csharp
public interface ITimeStepProvider
{
		long GetCurrentTimeStep();
}
```

### `SteamGuard`

- `SteamGuard()`
	Uses the default system-based time-step provider.
- `SteamGuard(ITimeStepProvider timeStepProvider)`
	Uses a custom provider (useful for tests and reproducible output).

## Deterministic Example (Testing)

```csharp
using SteamGuard.TOTP;

public sealed class FixedTimeStepProvider : ITimeStepProvider
{
		private readonly long timeStep;

		public FixedTimeStepProvider(long timeStep)
		{
				this.timeStep = timeStep;
		}

		public long GetCurrentTimeStep() => timeStep;
}

ISteamGuard steamGuard = new SteamGuard(new FixedTimeStepProvider(613));
string code = steamGuard.GenerateAuthenticationCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");
// code == "D57RM"
```

## Input Notes

- `totpKey` is expected to be a Base32 secret.
- The current implementation does not perform explicit input validation.
- From the current test suite:
	- Passing `""` throws `InvalidOperationException`.
	- Passing `null` throws `NullReferenceException`.

If strict validation behavior is needed, perform input checks before calling the library.

## Build and Test

From the repository root:

```bash
dotnet restore
dotnet build
dotnet test
```

## Contributing

Contributions are welcome.

When contributing:

- keep the project cross-platform
- preserve the existing public API unless a breaking change is intentional
- keep changes focused and consistent with the current coding style
- update documentation when behavior changes

## Related Projects

- [steam-totp](https://github.com/hmlendea/steam-totp): CLI app for generating Steam Guard codes.
- [steamguard.totp](https://github.com/hmlendea/steamguard.totp): This NuGet library.

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
