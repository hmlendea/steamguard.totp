# Repository Overview

## Purpose

SteamGuard.TOTP is a lightweight .NET library that generates Steam Guard authentication codes from a Base32-encoded TOTP shared secret. It implements the Steam-specific variant of RFC 6238 (TOTP) with HMAC-SHA1 and a 5-character output using the Steam character alphabet.

## Scope

**In scope:**
- Base32 secret decoding (RFC 4648 alphabet, no padding)
- Time-step calculation (30-second intervals, Unix epoch)
- HMAC-SHA1 computation of time step using secret as key
- Dynamic truncation (RFC 4226) to extract 4-byte code
- Mapping to 5-character Steam alphabet: `23456789BCDFGHJKMNPQRTVWXY`
- Dependency injection via `ITimeStepProvider` for testability

**Out of scope:**
- Network communication (no HTTP, no Steam API calls)
- Secret storage or management
- QR code generation
- Backup codes
- Multi-device synchronization
- Any persistence or caching
- Input validation (caller responsibility)

## Entry Points

### Public API

| Entry Point | Location | Description |
|-------------|----------|-------------|
| `ISteamGuard.GenerateAuthenticationCode(string totpKey)` | `SteamGuard.TOTP/ISteamGuard.cs` | Primary public method; generates 5-char code from Base32 secret |
| `SteamGuard()` | `SteamGuard.TOTP/SteamGuard.cs` | Default constructor; uses `TimeStepProvider` |
| `SteamGuard(ITimeStepProvider)` | `SteamGuard.TOTP/SteamGuard.cs` | DI constructor; accepts custom time provider |

### Test Entry Points

| Entry Point | Location | Description |
|-------------|----------|-------------|
| `FixedTimeStepProvider(long value)` | `SteamGuard.TOTP.UnitTests/Utils/FixedTimeStepProvider.cs` | Test double for deterministic time steps |

## Runtime Topology

```
Consuming Application
        │
        ▼
┌───────────────────────┐
│   SteamGuard.TOTP     │
│  (in-process library) │
├───────────────────────┤
│  ISteamGuard          │◄── Public contract
│  SteamGuard           │    Implementation
│  ITimeStepProvider    │◄── Time abstraction
│  TimeStepProvider     │    Default impl (uses NuciExtensions)
└───────────────────────┘
        │
        ▼
┌───────────────────────┐
│  External Dependencies│
│  • NuciExtensions     │  (compile-time only)
│  • System.Security.   │  (BCL - HMACSHA1)
│    Cryptography       │
└───────────────────────┘
```

## Key Characteristics

- **Stateless:** No instance or static state retained between calls
- **Thread-safe:** Pure functions with no shared mutable state
- **No I/O:** No file, network, or console access
- **No logging:** No telemetry, diagnostics, or audit trails
- **Single target framework:** `net10.0` only
- **GPL-3.0-or-later:** Copyleft license