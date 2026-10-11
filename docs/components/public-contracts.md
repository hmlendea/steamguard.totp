# Public Contracts

This document describes the public interfaces that define the library's API surface.

## ISteamGuard Interface

**Location:** `SteamGuard.TOTP/ISteamGuard.cs`
**Namespace:** `SteamGuard.TOTP`
**Accessibility:** `public`

### Definition

```csharp
public interface ISteamGuard
{
    string GenerateAuthenticationCode(string totpKey);
}
```

### Method: GenerateAuthenticationCode

**Signature:** `string GenerateAuthenticationCode(string totpKey)`

**Purpose:** Generates a 5-character Steam Guard authentication code from a Base32-encoded TOTP shared secret.

**Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `totpKey` | `string` | Base32-encoded TOTP shared secret (RFC 4648 alphabet, no padding) |

**Returns:**
| Type | Description |
|------|-------------|
| `string` | 5-character authentication code from Steam alphabet `23456789BCDFGHJKMNPQRTVWXY` |

**Exceptions:**
| Exception | Condition |
|-----------|-----------|
| `NullReferenceException` | `totpKey` is null |
| `InvalidOperationException` | `totpKey` is empty string |
| `ArgumentException` | `totpKey` contains characters not in Base32 alphabet |

**Thread safety:** Implementation must be thread-safe (stateless contract)

**Idempotency:** Yes — same input always produces same output for same time step

**Side effects:** None — pure computation

### Contract Invariants

1. Output is always exactly 5 characters
2. Output characters are always from `23456789BCDFGHJKMNPQRTVWXY`
3. Output depends on both input secret and current time step
4. No network, file, or console I/O
5. No state mutation

## ITimeStepProvider Interface

**Location:** `SteamGuard.TOTP/ITimeStepProvider.cs`
**Namespace:** `SteamGuard.TOTP`
**Accessibility:** `public`

### Definition

```csharp
public interface ITimeStepProvider
{
    long GetCurrentTimeStep();
}
```

### Method: GetCurrentTimeStep

**Signature:** `long GetCurrentTimeStep()`

**Purpose:** Returns the current TOTP time step (number of 30-second intervals since Unix epoch).

**Returns:**
| Type | Description |
|------|-------------|
| `long` | Current time step (Unix epoch ticks / 30 seconds) |

**Thread safety:** Implementation must be thread-safe

**Determinism:** For a given instant, returns the same value

### Contract Invariants

1. Returns non-negative value for dates after Unix epoch
2. Increments by 1 every 30 seconds
3. No side effects
4. No I/O operations

## Implementation Mapping

| Interface | Implementation | Location |
|-----------|----------------|----------|
| `ISteamGuard` | `SteamGuard` | `SteamGuard.TOTP/SteamGuard.cs` |
| `ITimeStepProvider` | `TimeStepProvider` | `SteamGuard.TOTP/TimeStepProvider.cs` |
| `ITimeStepProvider` | `FixedTimeStepProvider` | `SteamGuard.TOTP.UnitTests/Utils/FixedTimeStepProvider.cs` |

## Usage Patterns

### Default (Production)

```csharp
ISteamGuard steamGuard = new SteamGuard();
string code = steamGuard.GenerateAuthenticationCode("JBSWY3DPEHPK3PXP");
```

### Dependency Injection

```csharp
// Register in DI container
services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
services.AddTransient<ISteamGuard, SteamGuard>();

// Consume
public class MyService
{
    private readonly ISteamGuard steamGuard;
    public MyService(ISteamGuard steamGuard) => this.steamGuard = steamGuard;
    
    public string GetCode(string secret) => steamGuard.GenerateAuthenticationCode(secret);
}
```

### Testing (Deterministic)

```csharp
ITimeStepProvider fixedProvider = new FixedTimeStepProvider(613);
ISteamGuard steamGuard = new SteamGuard(fixedProvider);
string code = steamGuard.GenerateAuthenticationCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");
// code == "D57RM"
```

### Mocking (Unit Tests)

```csharp
var mock = new Mock<ITimeStepProvider>();
mock.Setup(x => x.GetCurrentTimeStep()).Returns(123456789L);
var steamGuard = new SteamGuard(mock.Object);
```

## Versioning Guarantees

| Contract | Stability | Change Policy |
|----------|-----------|---------------|
| `ISteamGuard.GenerateAuthenticationCode` | Stable | Breaking change requires major version bump |
| `ITimeStepProvider.GetCurrentTimeStep` | Stable | Breaking change requires major version bump |
| `SteamGuard` constructors | Stable | New overloads permitted; existing preserved |
| `TimeStepProvider` | Stable | Internal detail; may change without notice |
| `FixedTimeStepProvider` | Test-only | No stability guarantee |

## Compatibility

- **Binary compatibility:** Maintained within minor versions
- **Source compatibility:** Maintained within minor versions
- **Behavioral compatibility:** Same input + same time step → same output (guaranteed)
- **Serialization:** No serialization contracts; interfaces are not serializable