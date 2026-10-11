# ISteamGuard Reference

**Namespace:** `SteamGuard.TOTP`
**Assembly:** `SteamGuard.TOTP.dll`
**Package:** `SteamGuard.TOTP` 1.1.1

## Interface Definition

```csharp
public interface ISteamGuard
{
    string GenerateAuthenticationCode(string totpKey);
}
```

## Method: GenerateAuthenticationCode

### Signature

```csharp
string GenerateAuthenticationCode(string totpKey);
```

### Description

Generates a 5-character Steam Guard authentication code from a Base32-encoded TOTP shared secret. This method implements the Steam-specific variant of RFC 6238 (TOTP) using HMAC-SHA1 with a 30-second time step and the Steam character alphabet.

### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `totpKey` | `string` | Yes | Base32-encoded TOTP shared secret using RFC 4648 alphabet (`A-Z`, `2-7`), no padding |

### Returns

| Type | Description |
|------|-------------|
| `string` | 5-character authentication code using Steam alphabet: `23456789BCDFGHJKMNPQRTVWXY` |

### Exceptions

| Exception | Condition |
|-----------|-----------|
| `NullReferenceException` | `totpKey` is `null` |
| `InvalidOperationException` | `totpKey` is empty string (`""`) |
| `ArgumentException` | `totpKey` contains characters not in the Base32 alphabet |

### Remarks

- **Thread-safe:** Implementations must be thread-safe (the default `SteamGuard` implementation is stateless)
- **Deterministic for fixed time:** Same `totpKey` + same time step → same output
- **Time-dependent:** Output changes every 30 seconds
- **No I/O:** No network, file, or console access
- **No side effects:** Pure computation
- **No validation:** Caller should validate Base32 format before calling

### Algorithm Overview

1. **Base32 Decode:** Convert `totpKey` from RFC 4648 Base32 to byte array
2. **Time Step:** Get current 30-second interval since Unix epoch via `ITimeStepProvider`
3. **HMAC-SHA1:** Compute HMAC-SHA1 of time step (big-endian) using decoded secret as key
4. **Dynamic Truncation:** Extract 31-bit integer per RFC 4226
5. **Alphabet Mapping:** Convert to 5-character code using Steam alphabet (base-32)

### Examples

#### Basic Usage

```csharp
using SteamGuard.TOTP;

ISteamGuard steamGuard = new SteamGuard();
string sharedSecret = "JBSWY3DPEHPK3PXP"; // Example Base32 secret
string code = steamGuard.GenerateAuthenticationCode(sharedSecret);
Console.WriteLine(code); // e.g., "D57RM"
```

#### Dependency Injection

```csharp
// Registration
services.AddTransient<ISteamGuard, SteamGuard>();

// Consumption
public class SteamAuthService
{
    private readonly ISteamGuard steamGuard;

    public SteamAuthService(ISteamGuard steamGuard)
    {
        this.steamGuard = steamGuard;
    }

    public string GenerateCode(string sharedSecret)
    {
        return steamGuard.GenerateAuthenticationCode(sharedSecret);
    }
}
```

#### Testing with Fixed Time

```csharp
using SteamGuard.TOTP;
using SteamGuard.TOTP.UnitTests.Utils; // Test project only

// Deterministic test
ITimeStepProvider fixedProvider = new FixedTimeStepProvider(613);
ISteamGuard steamGuard = new SteamGuard(fixedProvider);
string code = steamGuard.GenerateAuthenticationCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");
// code == "D57RM" (always, for time step 613)
```

#### Mocking in Unit Tests

```csharp
using Moq;
using SteamGuard.TOTP;

var mockProvider = new Mock<ITimeStepProvider>();
mockProvider.Setup(x => x.GetCurrentTimeStep()).Returns(123456789L);

ISteamGuard steamGuard = new SteamGuard(mockProvider.Object);
string code = steamGuard.GenerateAuthenticationCode("JBSWY3DPEHPK3PXP");
// code depends on mocked time step
```

### Input Format Requirements

The `totpKey` parameter must be a valid Base32 string per RFC 4648:

| Requirement | Detail |
|-------------|--------|
| **Alphabet** | `A-Z`, `2-7` (32 characters) |
| **Case** | Case-insensitive (converted to uppercase internally) |
| **Padding** | Not supported (no `=` padding) |
| **Length** | Multiple of 8 characters (each 8 chars = 5 bytes) |
| **Typical length** | 16 chars (10 bytes), 26 chars (16 bytes), 32 chars (20 bytes) |

**Valid examples:**
- `"JBSWY3DPEHPK3PXP"` (16 chars, 10 bytes)
- `"DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3"` (32 chars, 20 bytes)

**Invalid examples:**
- `"JBSWY3DPEHPK3PXP="` (contains padding)
- `"JBSWY3DPEHPK3PX!"` (contains invalid character `!`)
- `"jbswy3dpehpK3pxp"` (valid after `.ToUpper()`)

### Output Format

| Property | Value |
|----------|-------|
| **Length** | Exactly 5 characters |
| **Alphabet** | `23456789BCDFGHJKMNPQRTVWXY` (32 chars) |
| **Excluded chars** | `0, 1, 8, I, L, O, U, Z` (ambiguous characters) |
| **Case** | Uppercase only |

### Compatibility

| Aspect | Guarantee |
|--------|-----------|
| **Steam compatibility** | Matches Steam Guard mobile authenticator output |
| **RFC compliance** | RFC 6238 (TOTP) with RFC 4226 (HOTP) truncation |
| **Algorithm** | HMAC-SHA1 (fixed, not configurable) |
| **Time step** | 30 seconds (fixed, not configurable) |
| **Code length** | 5 characters (fixed, not configurable) |

### Versioning

| Version | Changes |
|---------|---------|
| 1.0.0 | Initial release |
| 1.1.0 | Added `ITimeStepProvider` abstraction |
| 1.1.1 | Current version |

**Breaking change policy:** Changes to this interface require major version bump.

### See Also

- [ITimeStepProvider Reference](./itimestep-provider.md) — Time abstraction interface
- [SteamGuard Class](../components/core-implementation.md#steamguard-class) — Default implementation
- [Code Generation Flow](../flows/code-generation.md) — Detailed execution sequence
- [Architecture](../architecture.md) — Component relationships