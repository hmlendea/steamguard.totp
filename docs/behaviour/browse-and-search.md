# Browse and Search Behaviour

## Overview

This document describes the browse and search capabilities of the SteamGuard.TOTP library. Since this is a minimal library with a single public API surface, "browsing" and "searching" refer to how consumers discover and use the library's functionality.

## API Discovery

### Public Types

The library exposes only two public types:

| Type | Kind | Purpose |
|------|------|---------|
| `ISteamGuard` | Interface | Primary contract for code generation |
| `ITimeStepProvider` | Interface | Time step abstraction for testability |
| `SteamGuard` | Class | Default implementation of `ISteamGuard` |
| `TimeStepProvider` | Class | Default implementation of `ITimeStepProvider` |

### Namespace

All types reside in the `SteamGuard.TOTP` namespace.

```csharp
using SteamGuard.TOTP;
```

## Browsing the API

### IDE IntelliSense

The library is designed for discoverability via IDE IntelliSense:

1. Type `new SteamGuard(` → see constructor overloads
2. Type `steamGuard.` → see `GenerateAuthenticationCode` method
3. Type `new TimeStepProvider(` → see parameterless constructor
4. Type `provider.` → see `GetCurrentTimeStep` method

### Documentation Comments

All public members have XML documentation comments:

```csharp
/// <summary>
/// Generates a Steam Guard authentication code from a Base32-encoded shared secret.
/// </summary>
/// <param name="sharedSecret">Base32-encoded shared secret (case-insensitive).</param>
/// <returns>5-character authentication code using Steam alphabet.</returns>
/// <exception cref="ArgumentNullException">Thrown when sharedSecret is null.</exception>
/// <exception cref="ArgumentException">Thrown when sharedSecret is empty or invalid Base32.</exception>
public string GenerateAuthenticationCode(string sharedSecret)
```

## Searching for Functionality

### By Use Case

| Use Case | Entry Point |
|----------|-------------|
| Generate Steam Guard code | `SteamGuard.GenerateAuthenticationCode` |
| Custom time source | Implement `ITimeStepProvider` |
| Unit testing | Use `FixedTimeStepProvider` |
| Dependency injection | Register `ISteamGuard` and `ITimeStepProvider` |

### By Method

| Method | Class/Interface | Description |
|--------|-----------------|-------------|
| `GenerateAuthenticationCode` | `ISteamGuard`, `SteamGuard` | Main entry point |
| `GetCurrentTimeStep` | `ITimeStepProvider`, `TimeStepProvider` | Time step retrieval |
| `EncodeToBase32` | `SteamGuard` (internal) | Base32 encoding |
| `ComputeTotp` | `SteamGuard` (internal) | TOTP computation |
| `GenerateCodeFromBinCode` | `SteamGuard` (internal) | Steam alphabet mapping |

## Filtering and Navigation

### Filtering by Visibility

| Visibility | Types |
|------------|-------|
| Public API | `ISteamGuard`, `ITimeStepProvider`, `SteamGuard`, `TimeStepProvider` |
| Internal | `SteamGuard.EncodeToBase32`, `SteamGuard.ComputeTotp`, `SteamGuard.GenerateCodeFromBinCode` |
| Test-only | `FixedTimeStepProvider` (in test project) |

### Navigation Paths

```
SteamGuard.TOTP (namespace)
├── ISteamGuard (interface)
│   └── GenerateAuthenticationCode(string) : string
├── SteamGuard (class)
│   ├── SteamGuard() : constructor
│   ├── SteamGuard(ITimeStepProvider) : constructor
│   └── GenerateAuthenticationCode(string) : string
├── ITimeStepProvider (interface)
│   └── GetCurrentTimeStep() : long
├── TimeStepProvider (class)
│   ├── TimeStepProvider() : constructor
│   └── GetCurrentTimeStep() : long
└── FixedTimeStepProvider (class, test project)
    ├── FixedTimeStepProvider(long) : constructor
    └── GetCurrentTimeStep() : long
```

## Search in Documentation

### Online Documentation

- **NuGet Package**: https://www.nuget.org/packages/SteamGuard.TOTP
- **GitHub Repository**: https://github.com/horatiu/steamguard.totp
- **API Reference**: See `docs/api-reference/INDEX.md`

### Local Documentation

```bash
# View API reference
cat docs/api-reference/INDEX.md

# View interface details
cat docs/api-reference/isteamguard.md
cat docs/api-reference/itimestep-provider.md

# View usage examples
cat docs/api-usage-examples.md
```

## Discoverability Features

### Minimal Surface Area

The library intentionally exposes minimal public API:
- 2 interfaces
- 2 concrete classes (1 in main lib, 1 in test project)
- 1 primary method (`GenerateAuthenticationCode`)
- 1 supporting method (`GetCurrentTimeStep`)

### Self-Documenting Names

| Name | Indicates |
|------|-----------|
| `ISteamGuard` | Steam Guard functionality |
| `ITimeStepProvider` | Provides time steps |
| `GenerateAuthenticationCode` | Generates auth codes |
| `GetCurrentTimeStep` | Gets current time step |
| `FixedTimeStepProvider` | Fixed time for testing |

## Limitations

### No Runtime Discovery

The library does not provide:
- Plugin system
- Dynamic method discovery
- Runtime type inspection beyond standard reflection
- Configuration-based behavior switching

### No Search API

There is no programmatic search API. Consumers use:
- IDE IntelliSense
- Documentation
- Source code (open source)

## Related Documents

- [API Reference Index](api-reference/INDEX.md)
- [ISteamGuard Reference](api-reference/isteamguard.md)
- [ITimeStepProvider Reference](api-reference/itimestep-provider.md)
- [API Usage Examples](api-usage-examples.md)
- [Quick Start](quick-start.md)