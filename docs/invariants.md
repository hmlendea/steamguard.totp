# Invariants

## Overview

This document describes the invariants (guaranteed properties) of the SteamGuard.TOTP library that must hold true across all versions and implementations.

## Algorithmic Invariants

### TOTP Algorithm (RFC 6238)

| Invariant | Description |
|-----------|-------------|
| **HMAC-SHA1** | Always uses HMAC-SHA1 as the MAC algorithm |
| **30-second interval** | Time step is always 30 seconds |
| **Unix epoch** | Time steps counted from 1970-01-01T00:00:00Z |
| **Dynamic truncation** | Uses RFC 4226 dynamic truncation method |
| **Modulo 10^7** | Binary code reduced modulo 10,000,000 |

### Steam-Specific Invariants

| Invariant | Description |
|-----------|-------------|
| **5-character output** | Always generates exactly 5 characters |
| **Steam alphabet** | Uses `23456789BCDFGHJKMNPQRTVWXY` (26 chars) |
| **Uppercase only** | Output is always uppercase |
| **No padding** | No padding characters in output |

## Interface Invariants

### ISteamGuard

```csharp
public interface ISteamGuard
{
    string GenerateAuthenticationCode(string sharedSecret);
}
```

| Invariant | Guarantee |
|-----------|-----------|
| **Not null return** | Never returns `null` for valid input |
| **Length 5** | Return value always has length 5 |
| **Valid characters** | Return value only contains Steam alphabet chars |
| **Deterministic** | Same secret + same time step = same code |
| **Thread-safe** | Concurrent calls are safe |

### ITimeStepProvider

```csharp
public interface ITimeStepProvider
{
    long GetCurrentTimeStep();
}
```

| Invariant | Guarantee |
|-----------|-----------|
| **Non-negative** | Returns value ≥ 0 |
| **Monotonic** | Real-time implementations never decrease |
| **30-second granularity** | Value changes at most once per 30 seconds |
| **Thread-safe** | Concurrent calls are safe |

## Implementation Invariants

### SteamGuard Class

| Invariant | Guarantee |
|-----------|-----------|
| **Stateless** | No instance fields that affect output |
| **Constructor injection** | Time provider set at construction only |
| **No side effects** | `GenerateAuthenticationCode` has no observable side effects |
| **No I/O** | No file, network, or console I/O |
| **No logging** | No logging of any kind |
| **No telemetry** | No metrics, tracing, or telemetry |

### TimeStepProvider Class

| Invariant | Guarantee |
|-----------|-----------|
| **System time** | Uses `DateTimeExtensions.GetElapsedUnixTime()` |
| **Stateless** | No instance state |
| **Thread-safe** | Safe for concurrent use |

### FixedTimeStepProvider Class

| Invariant | Guarantee |
|-----------|-----------|
| **Immutable** | Time step fixed at construction |
| **Deterministic** | Always returns same value |
| **Thread-safe** | Safe for concurrent use |

## Input/Output Invariants

### Input Validation

| Input | Invariant |
|-------|-----------|
| `null` secret | Throws `ArgumentNullException` |
| Empty secret | Throws `ArgumentException` |
| Invalid Base32 | Throws `FormatException` |
| Valid Base32 | Accepts and processes |

### Output Format

| Property | Invariant |
|----------|-----------|
| Type | `string` |
| Length | Exactly 5 |
| Characters | Subset of `23456789BCDFGHJKMNPQRTVWXY` |
| Case | Uppercase |
| Null | Never null |

## Time Invariants

### Time Step Calculation

```
timeStep = floor(unixTimeSeconds / 30)
```

| Property | Invariant |
|----------|-----------|
| Interval | Exactly 30 seconds |
| Epoch | Unix epoch (1970-01-01T00:00:00Z) |
| Direction | Increases with time |
| Granularity | Changes at :00 and :30 of each minute |

### Code Validity

| Property | Invariant |
|----------|-----------|
| Validity window | Current 30-second interval |
| Boundary | Changes at exact 30-second boundaries |
| Overlap | No overlap between intervals |

## Cryptographic Invariants

### HMAC-SHA1

| Property | Invariant |
|----------|-----------|
| Algorithm | HMAC-SHA1 (FIPS 198-1) |
| Key | Base32-decoded shared secret |
| Message | 8-byte big-endian time step |
| Output | 20 bytes (160 bits) |

### Truncation

| Property | Invariant |
|----------|-----------|
| Method | RFC 4226 dynamic truncation |
| Offset | Last 4 bits of hash[19] |
| Bytes extracted | 4 bytes starting at offset |
| Sign bit | Masked (0x7F on first byte) |
| Result | 31-bit unsigned integer |

### Modulo

| Property | Invariant |
|----------|-----------|
| Modulus | 10,000,000 (10^7) |
| Range | 0 to 9,999,999 |

### Alphabet Mapping

| Property | Invariant |
|----------|-----------|
| Alphabet | `23456789BCDFGHJKMNPQRTVWXY` |
| Length | 26 characters |
| Mapping | Base-26 conversion |
| Direction | Least significant digit first |

## Dependency Invariants

### NuciExtensions

| Property | Invariant |
|----------|-----------|
| Version | 5.3.1 (pinned) |
| Usage | `DateTimeExtensions.GetElapsedUnixTime()` only |
| Compile-time only | No runtime dependency beyond BCL |

### .NET BCL

| Property | Invariant |
|----------|-----------|
| Target | net10.0 |
| Cryptography | `System.Security.Cryptography.HMACSHA1` |
| Network | `System.Net.IPAddress.HostToNetworkOrder` |
| Base | `System.Convert`, `System.BitConverter` |

## Testing Invariants

### Known Vectors

The following test vectors must always pass:

| Secret | Time Step | Expected Code |
|--------|-----------|---------------|
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 55752480 | `R73NH` |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 0 | `24682` |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 1 | `87654` |

### Test Coverage

| Area | Invariant |
|------|-----------|
| Known vectors | All must pass |
| Code length | Always 5 |
| Character set | Only Steam alphabet |
| Null input | Throws `ArgumentNullException` |
| Empty input | Throws `ArgumentException` |
| Invalid Base32 | Throws `FormatException` |

## Versioning Invariants

### Public API

| Element | Invariant |
|---------|-----------|
| `ISteamGuard` | Never removed or changed |
| `ITimeStepProvider` | Never removed or changed |
| `SteamGuard` constructors | Never removed |
| `TimeStepProvider` constructor | Never removed |
| Steam alphabet | Never changed |
| Time step interval | Never changed |
| Code length | Never changed |
| HMAC algorithm | Never changed |

### Breaking Changes

Only allowed with major version bump:
- Adding new interface methods
- Changing method signatures
- Removing public types
- Changing exception types

## Performance Invariants

| Metric | Invariant |
|--------|-----------|
| Allocation | < 1 KB per call |
| Time | < 1 ms per call |
| Scaling | Linear with concurrency |
| Memory | No leaks |

## Security Invariants

| Property | Invariant |
|----------|-----------|
| No secret logging | Never logs shared secret |
| No code logging | Never logs generated codes |
| No network calls | Zero network I/O |
| No file I/O | Zero file system access |
| No telemetry | Zero telemetry collection |
| Constant-time | HMAC computation is constant-time |

## Compatibility Invariants

### Steam Compatibility

| Property | Invariant |
|----------|-----------|
| Algorithm match | Matches Steam's TOTP implementation |
| Alphabet match | Matches Steam's 26-char alphabet |
| Time step match | Matches Steam's 30-second interval |
| Code length match | Matches Steam's 5-char codes |

### .NET Compatibility

| Property | Invariant |
|----------|-----------|
| Target framework | net10.0 |
| Nullable | Enabled |
| Implicit usings | Enabled |
| Documentation | XML docs generated |

## Invariant Verification

### Automated Checks

```bash
# Run tests to verify invariants
dotnet test

# Check for breaking changes
dotnet build -warnaserror

# Verify package
dotnet pack --configuration Release
```

### Manual Verification

```csharp
// Verify output format
string code = steamGuard.GenerateAuthenticationCode(secret);
Debug.Assert(code.Length == 5);
Debug.Assert(code.All(c => "23456789BCDFGHJKMNPQRTVWXY".Contains(c)));

// Verify determinism
string code1 = steamGuard.GenerateAuthenticationCode(secret);
string code2 = steamGuard.GenerateAuthenticationCode(secret);
Debug.Assert(code1 == code2);
```

## Invariant Violation Response

If an invariant is violated:

1. **Immediate**: Revert change
2. **Investigate**: Root cause analysis
3. **Test**: Add regression test
4. **Document**: Update this file if invariant changes
5. **Version**: Major version bump if public API invariant changed

## Related Documents

- [Design Decisions](design-decisions.md)
- [Core Implementation](../components/core-implementation.md)
- [Code Generation Flow](../flows/code-generation.md)
- [Testing](testing.md)