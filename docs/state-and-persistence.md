# State and Persistence

## Overview

This document describes the state management and persistence behavior of the SteamGuard.TOTP library.

## State Model

### No Persistent State

The library maintains **zero persistent state**:

| State Type | Present? | Details |
|------------|----------|---------|
| Database | ❌ | No database connections |
| File storage | ❌ | No file I/O |
| Cache | ❌ | No caching layer |
| Session | ❌ | No session management |
| Configuration | ❌ | No config files |
| Telemetry | ❌ | No metrics storage |

### Ephemeral Computation State

Each call to `GenerateAuthenticationCode` creates temporary state:

```
Call Start
    │
    ▼
┌─────────────────────┐
│ Input: sharedSecret │  ← Parameter (caller-owned)
└─────────────────────┘
    │
    ▼
┌─────────────────────┐
│ Decode Base32       │  ← byte[] (local, GC'd)
└─────────────────────┘
    │
    ▼
┌─────────────────────┐
│ Get time step       │  ← long (local, GC'd)
└─────────────────────┘
    │
    ▼
┌─────────────────────┐
│ HMAC-SHA1 compute   │  ← byte[20] (local, GC'd)
└─────────────────────┘
    │
    ▼
┌─────────────────────┐
│ Truncate & map      │  ← int, string (local, GC'd)
└─────────────────────┘
    │
    ▼
Return code (string)  ← Caller-owned
Call End
```

## Object Lifecycle

### SteamGuard

```csharp
// Creation
var steamGuard = new SteamGuard();           // Stateless
var steamGuard = new SteamGuard(provider);   // Stateless, holds provider reference

// Usage (stateless)
string code = steamGuard.GenerateAuthenticationCode(secret);

// Disposal
// No IDisposable, no cleanup needed
// Eligible for GC when out of scope
```

### TimeStepProvider

```csharp
// Creation
var provider = new TimeStepProvider();  // Stateless

// Usage (stateless)
long step = provider.GetCurrentTimeStep();

// Disposal
// No IDisposable, no cleanup needed
```

### FixedTimeStepProvider

```csharp
// Creation (immutable)
var provider = new FixedTimeStepProvider(123456789L);

// Usage (stateless)
long step = provider.GetCurrentTimeStep();  // Always returns 123456789L

// Disposal
// No IDisposable, no cleanup needed
```

## Memory Management

### Allocation Profile

| Operation | Allocations | Size |
|-----------|-------------|------|
| `GenerateAuthenticationCode` | ~5-8 | ~200-500 bytes |
| `GetCurrentTimeStep` | 0 | 0 bytes |
| Base32 decode | 1 byte[] | ~20-40 bytes |
| HMAC-SHA1 | 1 byte[20] | 20 bytes |
| Time step bytes | 1 byte[8] | 8 bytes |
| Output string | 1 string | ~10 bytes (5 chars UTF-16) |

### No Memory Leaks

- No static fields holding references
- No event handlers
- No timers
- No unmanaged resources
- All allocations are short-lived (Gen 0)

## Thread Safety and State

### No Shared Mutable State

```
Thread 1                    Thread 2
    │                          │
    ▼                          ▼
┌─────────┐               ┌─────────┐
│ SteamGuard instance     │ (same or different)
│  (stateless)            │
└─────────┘               └─────────┘
    │                          │
    ├── secret (param)        ├── secret (param)
    ├── timeStep (local)      ├── timeStep (local)
    ├── hash (local)          ├── hash (local)
    └── code (local)          └── code (local)
    │                          │
    ▼                          ▼
Returns code              Returns code
```

### Safe for Concurrent Use

```csharp
// Single instance, multiple threads
var steamGuard = new SteamGuard();

Parallel.For(0, 100, _ => {
    string code = steamGuard.GenerateAuthenticationCode(secret);
    // Each call independent, no interference
});
```

## Persistence Patterns (Consumer Responsibility)

### Storing Shared Secrets

```csharp
// Consumer must handle persistence
public interface ISecretStore
{
    Task<string> GetSecretAsync(string accountId);
    Task StoreSecretAsync(string accountId, string secret);
}

// Implementations:
// - Azure Key Vault
// - HashiCorp Vault
// - AWS Secrets Manager
// - Encrypted database
// - Encrypted file
```

### Caching Codes (Not Recommended)

```csharp
// If needed, consumer can cache
public class CachedSteamGuard : ISteamGuard
{
    private readonly ISteamGuard _inner;
    private readonly IMemoryCache _cache;

    public string GenerateAuthenticationCode(string sharedSecret)
    {
        string cacheKey = $"steam-code-{sharedSecret.GetHashCode()}";

        if (_cache.TryGetValue(cacheKey, out string cached))
            return cached;

        string code = _inner.GenerateAuthenticationCode(sharedSecret);

        // Cache for remainder of 30-second window
        var expiry = DateTimeOffset.UtcNow.AddSeconds(30 - DateTimeOffset.UtcNow.Second % 30);
        _cache.Set(cacheKey, code, expiry);

        return code;
    }
}
```

## Serialization

### No Built-in Serialization

The library does not provide:
- JSON serialization
- XML serialization
- Binary serialization
- Custom formatters

### Consumer Serialization

```csharp
// If consumer needs to serialize
public record SteamGuardConfig
{
    public string SharedSecret { get; init; }
    public long? FixedTimeStep { get; init; }
}

// JSON
string json = JsonSerializer.Serialize(config);

// Deserialize
var config = JsonSerializer.Deserialize<SteamGuardConfig>(json);
```

## Testing State

### Deterministic Testing

```csharp
// FixedTimeStepProvider provides deterministic state
[Test]
public void TestWithFixedState()
{
    var provider = new FixedTimeStepProvider(55752480);
    var steamGuard = new SteamGuard(provider);

    // Always returns same code for same input
    string code1 = steamGuard.GenerateAuthenticationCode(secret);
    string code2 = steamGuard.GenerateAuthenticationCode(secret);

    Assert.That(code1, Is.EqualTo(code2));
}
```

### No State to Reset

```csharp
// No setup/teardown needed for state
[SetUp]
public void Setup() { }  // Empty - no state

[TearDown]
public void Teardown() { }  // Empty - no state
```

## Related Documents

- [Core Implementation](../components/core-implementation.md)
- [Concurrency and Scheduling](concurrency-and-scheduling.md)
- [Data Model](data-model.md)
- [Testing](testing.md)