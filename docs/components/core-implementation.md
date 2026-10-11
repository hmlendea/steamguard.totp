# Core Implementation Deep Dive

This document provides implementation-level detail for the `SteamGuard` and `TimeStepProvider` classes.

## SteamGuard Class

**Location:** `SteamGuard.TOTP/SteamGuard.cs`
**Namespace:** `SteamGuard.TOTP`
**Accessibility:** `public sealed`

### Fields

| Field | Type | Description |
|-------|------|-------------|
| `CodeLength` | `static readonly int` | Constant 5 (Steam Guard code length) |
| `AllowedTotpKeyCharacters` | `static readonly string` | Base32 alphabet: `ABCDEFGHIJKLMNOPQRSTUVWXYZ234567` |
| `AuthenticationCodeAlphabet` | `static readonly string` | Steam alphabet: `23456789BCDFGHJKMNPQRTVWXY` |
| `timeStepProvider` | `readonly ITimeStepProvider` | Injected time abstraction |

### Constructors

```csharp
public SteamGuard() : this(new TimeStepProvider()) { }

public SteamGuard(ITimeStepProvider timeStepProvider)
{
    this.timeStepProvider = timeStepProvider;
}
```

- **Default:** Uses `TimeStepProvider` (system clock via NuciExtensions)
- **DI:** Accepts any `ITimeStepProvider` implementation

### Public Method

#### `GenerateAuthenticationCode(string totpKey)`

```csharp
public string GenerateAuthenticationCode(string totpKey)
{
    byte[] securityToken = EncodeToBase32(totpKey);
    using HMACSHA1 hmacshA1 = new(securityToken);
    int binCode = ComputeTotp(hmacshA1);
    return GenerateCodeFromBinCode(binCode);
}
```

**Parameters:**
- `totpKey`: Base32-encoded TOTP shared secret (expected format, not validated)

**Returns:** 5-character Steam Guard authentication code

**Exceptions:**
- `NullReferenceException` if `totpKey` is null (from `ToUpper()`)
- `InvalidOperationException` if `totpKey` is empty (from `IndexOf` returning -1)
- `ArgumentException` if `totpKey` contains invalid Base32 characters

**Side effects:** None (pure computation)

### Private Methods

#### `EncodeToBase32(string source)`

Decodes RFC 4648 Base32 (no padding) to byte array.

```csharp
static byte[] EncodeToBase32(string source)
{
    string bits = source
        .ToUpper()
        .ToCharArray()
        .Select(c => Convert.ToString(AllowedTotpKeyCharacters.IndexOf(c), 2).PadLeft(5, '0'))
        .Aggregate((a, b) => a + b);

    return Enumerable
        .Range(0, bits.Length / 8)
        .Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2))
        .ToArray();
}
```

**Algorithm:**
1. Convert input to uppercase
2. For each character, find index in `AllowedTotpKeyCharacters`
3. Convert index to 5-bit binary string (padded left with zeros)
4. Concatenate all bit strings
5. Split into 8-bit chunks, convert each to byte

**Example:** `"JBSWY3DPEHPK3PXP"` (16 chars) → 80 bits → 10 bytes

**Edge cases:**
- Empty string → `bits.Length = 0` → `Range(0, 0)` → empty array → HMAC with empty key
- Invalid character → `IndexOf` returns -1 → `Convert.ToString(-1, 2)` → "-1" → exception

#### `ComputeTotp(HashAlgorithm algorithm)`

Computes TOTP value using HMAC-SHA1 and dynamic truncation (RFC 4226).

```csharp
int ComputeTotp(HashAlgorithm algorithm)
{
    long currentTimeStep = timeStepProvider.GetCurrentTimeStep();
    var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)currentTimeStep));
    var hmac = algorithm.ComputeHash(bytes);
    byte offset = (byte)(hmac[19] & 15);

    int binCode =
        ((hmac[offset] & 127) << 24) |
        ((hmac[offset + 1] & 255) << 16) |
        ((hmac[offset + 2] & 255) << 8) |
        (hmac[offset + 3] & 255);

    return binCode;
}
```

**Steps:**
1. Get current time step from provider
2. Convert to 8-byte big-endian (network order)
3. Compute HMAC-SHA1 (20 bytes)
4. Dynamic truncation:
   - Offset = low 4 bits of last byte (0-15)
   - Extract 4 bytes at offset
   - Mask MSB of first byte (31-bit positive integer)

**RFC 4226 compliance:** Implements standard HOTP dynamic truncation

#### `GenerateCodeFromBinCode(int binCode)`

Maps 31-bit integer to 5-character Steam alphabet code.

```csharp
static string GenerateCodeFromBinCode(int binCode)
{
    string code = string.Empty;
    for (int i = 0; i < CodeLength; ++i)
    {
        code += AuthenticationCodeAlphabet[binCode % AuthenticationCodeAlphabet.Length];
        binCode /= AuthenticationCodeAlphabet.Length;
    }
    return code;
}
```

**Algorithm:** Base-32 conversion using Steam alphabet (least significant digit first)

**Alphabet:** 32 chars → `binCode % 32` gives index, `binCode /= 32` shifts right

**Output:** Always 5 characters from `23456789BCDFGHJKMNPQRTVWXY`

## TimeStepProvider Class

**Location:** `SteamGuard.TOTP/TimeStepProvider.cs`
**Namespace:** `SteamGuard.TOTP`
**Accessibility:** `public sealed`

### Fields

| Field | Type | Description |
|-------|------|-------------|
| `Interval` | `static readonly TimeSpan` | Constant 30 seconds |

### Method

#### `GetCurrentTimeStep()`

```csharp
public long GetCurrentTimeStep()
    => DateTimeExtensions.GetElapsedUnixTime(DateTime.UtcNow).Ticks / Interval.Ticks;
```

**Returns:** Number of 30-second intervals since Unix epoch (Jan 1, 1970 UTC)

**Dependencies:**
- `DateTime.UtcNow` — current UTC time
- `NuciExtensions.DateTimeExtensions.GetElapsedUnixTime` — converts to `TimeSpan` since epoch

**Calculation:** `ticksSinceEpoch / (30 * TimeSpan.TicksPerSecond)`

**Thread safety:** Stateless; safe for concurrent use

## Test Double: FixedTimeStepProvider

**Location:** `SteamGuard.TOTP.UnitTests/Utils/FixedTimeStepProvider.cs`
**Namespace:** `SteamGuard.TOTP.UnitTests.Utils`
**Accessibility:** `public sealed`

### Constructor

```csharp
public FixedTimeStepProvider(long value)
{
    this.value = value;
}
```

### Method

```csharp
public long GetCurrentTimeStep() => value;
```

**Purpose:** Returns fixed time step for deterministic testing

**Usage in tests:**
```csharp
var provider = new FixedTimeStepProvider(613);
var steamGuard = new SteamGuard(provider);
var code = steamGuard.GenerateAuthenticationCode("DPNAMYILQFCAOTVS32XGGV3DSX5JYSP3");
// code == "D57RM"
```

## Implementation Notes

### Thread Safety

- `SteamGuard` is stateless (only readonly `timeStepProvider` field)
- `TimeStepProvider` is stateless
- `HMACSHA1` is created per-call (`using` statement)
- All methods are pure functions → inherently thread-safe

### Resource Management

- `HMACSHA1` implements `IDisposable` → wrapped in `using`
- No other unmanaged resources
- No finalizers needed

### Performance Characteristics

| Operation | Complexity | Allocations |
|-----------|------------|-------------|
| `EncodeToBase32` | O(n) where n = input length | String builder (bits), byte array |
| `ComputeTotp` | O(1) | byte[8] for time step, byte[20] for HMAC |
| `GenerateCodeFromBinCode` | O(1) fixed 5 iterations | String concatenation (5x) |

**Optimization opportunities:**
- `EncodeToBase32` uses string concatenation via `Aggregate` → could use `StringBuilder`
- `GenerateCodeFromBinCode` uses string `+=` → could use `StringBuilder` or `char[]`
- Current performance is adequate for typical usage (few calls per second)

### Known Limitations

1. **No input validation** — caller must ensure valid Base32
2. **Case-sensitive alphabet** — `AllowedTotpKeyCharacters` is uppercase only; input is converted via `ToUpper()`
3. **No padding support** — RFC 4648 padding (`=`) not handled
4. **Fixed algorithm** — HMAC-SHA1 only; no SHA-256/512 variants
5. **Fixed parameters** — 30s interval, 5 chars, Steam alphabet not configurable