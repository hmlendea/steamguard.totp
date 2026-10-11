# ITimeStepProvider Interface Reference

## Overview

| Property | Value |
|----------|-------|
| **Namespace** | `SteamGuard.TOTP` |
| **Assembly** | `SteamGuard.TOTP.dll` |
| **Package** | `SteamGuard.TOTP` |
| **Since** | 1.0.0 |

## Interface Definition

```csharp
public interface ITimeStepProvider
{
    long GetCurrentTimeStep();
}
```

## Method: GetCurrentTimeStep

### Signature

```csharp
long GetCurrentTimeStep();
```

### Description

Returns the current time step for TOTP calculation. A time step represents the number of 30-second intervals that have elapsed since the Unix epoch (January 1, 1970, 00:00:00 UTC).

### Returns

| Type | Description |
|------|-------------|
| `long` | Current time step value (number of 30-second intervals since Unix epoch) |

### Exceptions

| Exception | Condition |
|-----------|-----------|
| `Exception` | Implementation-specific; typically thrown if system time cannot be retrieved |

### Remarks

- **Time Step Calculation**: `timeStep = floor(currentUnixTime / 30)`
- **Interval**: Fixed at 30 seconds per RFC 6238
- **Epoch**: Unix epoch (1970-01-01T00:00:00Z)
- **Thread Safety**: Implementations must be thread-safe
- **Determinism**: For testing, implementations may return fixed values

## Implementations

### TimeStepProvider (Default)

```csharp
public class TimeStepProvider : ITimeStepProvider
{
    public long GetCurrentTimeStep()
    {
        return DateTimeExtensions.GetElapsedUnixTime() / 30;
    }
}
```

**Characteristics:**
- Uses `NuciExtensions.DateTimeExtensions.GetElapsedUnixTime()` for Unix time
- Returns current system time divided by 30
- Thread-safe (stateless)
- Production-ready

### FixedTimeStepProvider (Testing)

```csharp
public class FixedTimeStepProvider : ITimeStepProvider
{
    private readonly long _fixedTimeStep;

    public FixedTimeStepProvider(long fixedTimeStep)
    {
        _fixedTimeStep = fixedTimeStep;
    }

    public long GetCurrentTimeStep()
    {
        return _fixedTimeStep;
    }
}
```

**Characteristics:**
- Returns a fixed, configurable time step
- Used for deterministic testing
- Thread-safe (immutable)
- Test-only; not in production NuGet package

## Contract Invariants

| Invariant | Description |
|-----------|-------------|
| **Non-negative** | Return value must be ≥ 0 |
| **Monotonic** | Subsequent calls should return same or greater value (real-time implementations) |
| **30-second granularity** | Value changes only every 30 seconds (real-time implementations) |
| **Thread-safe** | Concurrent calls must not cause exceptions or corruption |

## Usage Patterns

### Default Usage (Production)

```csharp
var provider = new TimeStepProvider();
var steamGuard = new SteamGuard(provider);
string code = steamGuard.GenerateAuthenticationCode("base32secret");
```

### Dependency Injection

```csharp
// Startup.cs or Program.cs
services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
services.AddTransient<ISteamGuard, SteamGuard>();

// Controller/Service
public class AuthService
{
    private readonly ISteamGuard _steamGuard;

    public AuthService(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard;
    }

    public string GenerateCode(string secret) => _steamGuard.GenerateAuthenticationCode(secret);
}
```

### Testing with Fixed Time Step

```csharp
[Test]
public void GenerateCode_WithKnownTimeStep_ReturnsExpectedCode()
{
    // Time step for 2023-01-01T12:00:00Z = 1672574400 / 30 = 55752480
    var provider = new FixedTimeStepProvider(55752480);
    var steamGuard = new SteamGuard(provider);

    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");
    
    Assert.That(code, Is.EqualTo("R73NH"));
}
```

### Mocking with Moq

```csharp
[Test]
public void GenerateCode_UsesInjectedTimeStep()
{
    var mockProvider = new Mock<ITimeStepProvider>();
    mockProvider.Setup(p => p.GetCurrentTimeStep()).Returns(123456789L);

    var steamGuard = new SteamGuard(mockProvider.Object);
    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");

    Assert.That(code, Is.EqualTo("EXPECTED_CODE"));
    mockProvider.Verify(p => p.GetCurrentTimeStep(), Times.Once);
}
```

## Input/Output Format

### Time Step Values

| Unix Time | Time Step | Description |
|-----------|-----------|-------------|
| 0 | 0 | Unix epoch |
| 29 | 0 | First 30-second interval |
| 30 | 1 | Second interval |
| 59 | 1 | End of second interval |
| 1672574400 | 55752480 | 2023-01-01T12:00:00Z |
| 1704067200 | 56802240 | 2024-01-01T00:00:00Z |

### Example Time Steps

```csharp
// Current time step (approximate)
long current = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

// Specific timestamps
long epoch = 0L;                              // 1970-01-01T00:00:00Z
long y2k = 946684800L / 30;                   // 2000-01-01T00:00:00Z = 31556160
long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
```

## Compatibility

| Version | Compatibility |
|---------|---------------|
| 1.0.0+ | Stable interface; no breaking changes planned |
| Future | New methods may be added; existing method preserved |

## Versioning Guarantees

- **Interface Stability**: `GetCurrentTimeStep()` will not be removed or changed
- **Semantic Versioning**: Breaking changes require major version bump
- **Backward Compatibility**: New implementations work with existing consumers

## See Also

- [ISteamGuard Interface](isteamguard.md)
- [SteamGuard Class](../components/core-implementation.md#steamguard-class)
- [TimeStepProvider Class](../components/core-implementation.md#timestepprovider-class)
- [FixedTimeStepProvider Class](../components/core-implementation.md#fixedtimestepprovider-class)
- [Code Generation Flow](../flows/code-generation.md)
- [Test Execution Flow](../flows/test-execution.md)