# API Reference Index

This document provides a summary of all public API members in the SteamGuard.TOTP library.

## Library Information

| Property | Value |
|----------|-------|
| **Package ID** | `SteamGuard.TOTP` |
| **Version** | 1.1.1 |
| **Target Framework** | `net10.0` |
| **Namespace** | `SteamGuard.TOTP` |
| **License** | GPL-3.0-or-later |

## Public Types

| Type | Kind | Description |
|------|------|-------------|
| `ISteamGuard` | Interface | Primary public contract for Steam Guard code generation |
| `ITimeStepProvider` | Interface | Time abstraction for testability |
| `SteamGuard` | Class | Default implementation of `ISteamGuard` |
| `TimeStepProvider` | Class | Default implementation of `ITimeStepProvider` |

## Member Summary

### ISteamGuard

| Member | Signature | Description |
|--------|-----------|-------------|
| `GenerateAuthenticationCode` | `string GenerateAuthenticationCode(string totpKey)` | Generates 5-char Steam Guard code from Base32 secret |

### ITimeStepProvider

| Member | Signature | Description |
|--------|-----------|-------------|
| `GetCurrentTimeStep` | `long GetCurrentTimeStep()` | Returns current 30-second interval since Unix epoch |

### SteamGuard

| Member | Signature | Description |
|--------|-----------|-------------|
| Constructor | `SteamGuard()` | Creates instance with default `TimeStepProvider` |
| Constructor | `SteamGuard(ITimeStepProvider timeStepProvider)` | Creates instance with custom time provider |
| `GenerateAuthenticationCode` | `string GenerateAuthenticationCode(string totpKey)` | Implements `ISteamGuard.GenerateAuthenticationCode` |

### TimeStepProvider

| Member | Signature | Description |
|--------|-----------|-------------|
| `GetCurrentTimeStep` | `long GetCurrentTimeStep()` | Implements `ITimeStepProvider.GetCurrentTimeStep` using system clock |

## Quick Reference

### Basic Usage

```csharp
using SteamGuard.TOTP;

ISteamGuard steamGuard = new SteamGuard();
string code = steamGuard.GenerateAuthenticationCode("JBSWY3DPEHPK3PXP");
// code: "D57RM" (example, varies by time)
```

### Dependency Injection

```csharp
// Program.cs or Startup.cs
services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
services.AddTransient<ISteamGuard, SteamGuard>();

// Consumer
public class AuthService
{
    private readonly ISteamGuard steamGuard;
    public AuthService(ISteamGuard steamGuard) => this.steamGuard = steamGuard;
    
    public string GetSteamCode(string sharedSecret) => steamGuard.GenerateAuthenticationCode(sharedSecret);
}
```

### Testing

```csharp
using SteamGuard.TOTP;
using SteamGuard.TOTP.UnitTests.Utils;

ITimeStepProvider fixedProvider = new FixedTimeStepProvider(613);
ISteamGuard steamGuard = new SteamGuard(fixedProvider);
string code = steamGuard.GenerateAuthenticationCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");
// code == "D57RM" (deterministic)
```

## Related Documentation

- [ISteamGuard Reference](./isteamguard.md) — Detailed interface documentation
- [Architecture](../architecture.md) — Component relationships and runtime flow
- [Code Generation Flow](../flows/code-generation.md) — Detailed execution sequence
- [Testing](../testing.md) — Test strategy and verification