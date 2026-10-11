# Data Model

## Overview

This document describes the data model of the SteamGuard.TOTP library, including input/output formats, internal data structures, and data flow.

## Input Data

### Shared Secret

| Property | Value |
|----------|-------|
| **Type** | `string` |
| **Encoding** | Base32 (RFC 4648) |
| **Case** | Case-insensitive |
| **Padding** | Not required |
| **Example** | `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` |

#### Format

```
Base32 Alphabet: ABCDEFGHIJKLMNOPQRSTUVWXYZ234567
Steam Secret:    GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ
```

#### Validation Rules

| Rule | Exception |
|------|-----------|
| Not null | `ArgumentNullException` |
| Not empty | `ArgumentException` |
| Valid Base32 characters | `FormatException` |

### Time Step

| Property | Value |
|----------|-------|
| **Type** | `long` |
| **Unit** | 30-second intervals |
| **Epoch** | Unix epoch (1970-01-01T00:00:00Z) |
| **Example** | `55752480` (for 2023-01-01T12:00:00Z) |

#### Calculation

```
timeStep = floor(unixTimeSeconds / 30)
```

## Output Data

### Authentication Code

| Property | Value |
|----------|-------|
| **Type** | `string` |
| **Length** | 5 characters |
| **Alphabet** | `23456789BCDFGHJKMNPQRTVWXY` (26 chars) |
| **Case** | Uppercase |
| **Example** | `R73NH` |

#### Steam Alphabet

```
Position: 0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25
Char:     2  3  4  5  6  7  8  9  B  C  D  F  G  H  J  K  M  N  P  Q  R  T  V  W  X  Y
```

**Excluded characters**: `0, 1, A, E, I, L, O, S, U, Z` (to avoid visual confusion)

## Internal Data Structures

### Base32 Decoded Secret

| Property | Value |
|----------|-------|
| **Type** | `byte[]` |
| **Length** | Variable (typically 20-40 bytes) |
| **Source** | Decoded from Base32 input |

#### Example

```
Input (Base32):  GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ
Output (bytes):  [0x00, 0x46, 0x6F, 0x6F, 0x42, 0x61, 0x72, ...]
```

### Time Step Bytes

| Property | Value |
|----------|-------|
| **Type** | `byte[8]` |
| **Encoding** | Big-endian (network byte order) |
| **Source** | `BitConverter.GetBytes(timeStep)` reversed |

#### Example

```
timeStep = 55752480
bytes = [0x00, 0x00, 0x00, 0x00, 0x03, 0x52, 0x9A, 0x80]
```

### HMAC-SHA1 Hash

| Property | Value |
|----------|-------|
| **Type** | `byte[20]` |
| **Algorithm** | HMAC-SHA1 |
| **Key** | Base32 decoded secret |
| **Message** | 8-byte time step |

### Truncated Hash

| Property | Value |
|----------|-------|
| **Type** | `int` (32-bit) |
| **Source** | Dynamic truncation of HMAC-SHA1 |
| **Range** | 0 to 2,147,483,647 |

#### Truncation Process

```
1. Take last 4 bits of hash[19] as offset
2. Extract 4 bytes starting at offset
3. Mask first byte with 0x7F (remove sign bit)
4. Convert to 32-bit integer
```

### Binary Code

| Property | Value |
|----------|-------|
| **Type** | `int` |
| **Source** | `truncatedHash % 10000000` |
| **Range** | 0 to 9,999,999 |

## Data Flow

```mermaid
flowchart LR
    A[Shared Secret<br/>Base32 string] --> B[Decode Base32<br/>byte[]]
    C[Time Step<br/>long] --> D[Convert to bytes<br/>byte[8]]
    B --> E[HMAC-SHA1<br/>byte[20]]
    D --> E
    E --> F[Dynamic Truncation<br/>int]
    F --> G[Modulo 10,000,000<br/>int]
    G --> H[Map to Steam Alphabet<br/>string]
    H --> I[5-char Code<br/>e.g., R73NH]
```

## Data Transformations

### 1. Base32 Decode

```csharp
// Input: "GEZDGNBVGY3TQOJQ"
// Output: byte[] { 0x00, 0x46, 0x6F, 0x6F, 0x42, 0x61, 0x72, ... }

// Process:
// G -> 6, E -> 4, Z -> 7, D -> 3
// Combine: 000110 000100 000111 000011
// Result: 00011000 01000001 11000011 ...
```

### 2. Time Step to Bytes

```csharp
// Input: timeStep = 55752480
// long -> byte[8] big-endian

// Process:
// 55752480 in hex: 0x03529A80
// Big-endian bytes: [0x00, 0x00, 0x00, 0x00, 0x03, 0x52, 0x9A, 0x80]
```

### 3. HMAC-SHA1

```csharp
// Key: decoded secret bytes
// Message: 8-byte time step
// Output: 20-byte hash

using var hmac = new HMACSHA1(key);
byte[] hash = hmac.ComputeHash(message);
```

### 4. Dynamic Truncation

```csharp
// Input: 20-byte hash
// Output: 32-bit integer

int offset = hash[19] & 0xF;
int binary = (hash[offset] & 0x7F) << 24
           | (hash[offset + 1] & 0xFF) << 16
           | (hash[offset + 2] & 0xFF) << 8
           | (hash[offset + 3] & 0xFF);
```

### 5. Steam Alphabet Mapping

```csharp
// Input: binary % 10000000 (0-9,999,999)
// Output: 5-character string

const string steamAlphabet = "23456789BCDFGHJKMNPQRTVWXY";

string code = "";
int num = binary % 10000000;
for (int i = 0; i < 5; i++)
{
    code = steamAlphabet[num % steamAlphabet.Length] + code;
    num /= steamAlphabet.Length;
}
```

## Data Constraints

### Input Constraints

| Constraint | Value |
|------------|-------|
| Max secret length | Unlimited (practical: ~100 chars) |
| Min secret length | 1 character |
| Allowed characters | `A-Z`, `2-7` (Base32) |

### Output Constraints

| Constraint | Value |
|------------|-------|
| Code length | Exactly 5 |
| Code characters | `23456789BCDFGHJKMNPQRTVWXY` |
| Code case | Uppercase only |

### Time Constraints

| Constraint | Value |
|------------|-------|
| Time step interval | 30 seconds |
| Time step type | 64-bit signed integer |
| Minimum time step | 0 (Unix epoch) |
| Maximum time step | `long.MaxValue` |

## Data Persistence

### No Persistence

The library performs **no data persistence**:

| Aspect | Behavior |
|--------|----------|
| Database | None |
| File storage | None |
| Cache | None |
| Session state | None |
| Cookies | None |

### Ephemeral Data

All data exists only during method execution:

```csharp
// Data lifecycle:
// 1. Input: sharedSecret (string) - caller's responsibility
// 2. Processing: byte arrays, integers - stack/heap during call
// 3. Output: code (string) - caller's responsibility
// 4. After return: all intermediate data eligible for GC
```

## Data Security

### In-Memory Handling

| Data | Security |
|------|----------|
| Shared secret | Passed as string parameter |
| Decoded bytes | Temporary byte array |
| HMAC hash | Temporary byte array |
| Generated code | Returned as string |

### No Data Exposure

- No logging of secrets or codes
- No telemetry collection
- No network transmission
- No file I/O

## Data Examples

### Known Test Vectors

| Secret | Time Step | Expected Code |
|--------|-----------|---------------|
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 55752480 | `R73NH` |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 0 | `24682` |
| `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ` | 1 | `87654` |

### Data Size Summary

| Data | Size |
|------|------|
| Input secret (Base32) | ~20-40 chars |
| Decoded secret (bytes) | ~15-30 bytes |
| Time step (bytes) | 8 bytes |
| HMAC-SHA1 hash | 20 bytes |
| Truncated code (int) | 4 bytes |
| Output code (string) | 5 chars (10 bytes UTF-16) |

## Related Documents

- [Code Generation Flow](../flows/code-generation.md)
- [Core Implementation](../components/core-implementation.md)
- [Public Contracts](../components/public-contracts.md)
- [API Reference](../api-reference/INDEX.md)