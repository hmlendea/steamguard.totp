# Frequently Asked Questions

## General Questions

### What is SteamGuard.TOTP?

SteamGuard.TOTP is a .NET library that generates Steam Guard authentication codes using the TOTP (Time-based One-Time Password) algorithm as specified in RFC 6238, with Steam-specific parameters.

### What does it do?

It takes a Base32-encoded Steam shared secret and generates a 5-character authentication code using the Steam alphabet (`23456789BCDFGHJKMNPQRTVWXY`).

### Is this an official Steam library?

No. This is a community-created library based on reverse-engineering of Steam's TOTP implementation. It is not affiliated with Valve Corporation.

### What .NET versions are supported?

Currently: **net10.0** only. The library targets .NET 10.

## Usage Questions

### How do I get my Steam shared secret?

1. Use a Steam mobile authenticator extractor tool
2. Or use the Steam Desktop Authenticator
3. The secret is a Base32 string like `GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ`

### How do I use the library?

```csharp
var steamGuard = new SteamGuard();
string code = steamGuard.GenerateAuthenticationCode("YOUR_SHARED_SECRET");
```

### Can I use it with dependency injection?

Yes:
```csharp
services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
services.AddTransient<ISteamGuard, SteamGuard>();
```

### Does it work for multiple accounts?

Yes, create separate `SteamGuard` instances or call `GenerateAuthenticationCode` with different secrets.

## Technical Questions

### What algorithm does it use?

- **TOTP**: RFC 6238
- **HMAC**: HMAC-SHA1 (not SHA-256/512)
- **Time step**: 30 seconds
- **Code length**: 5 characters
- **Alphabet**: Steam-specific 26-character alphabet

### Why HMAC-SHA1 and not SHA-256?

Steam's implementation uses HMAC-SHA1. The library matches Steam's algorithm exactly for compatibility.

### Why 5 characters instead of 6?

Steam uses a 5-character code with a custom 26-character alphabet, not the standard 6-digit numeric TOTP.

### Is it thread-safe?

Yes. All public types (`SteamGuard`, `TimeStepProvider`, `FixedTimeStepProvider`) are stateless and thread-safe.

### Does it cache codes?

No. Each call computes a fresh code. Codes are valid for the current 30-second time step.

### What happens if my clock is wrong?

Codes will be generated for the wrong time step and won't work with Steam. Ensure your system clock is synchronized (NTP).

## Error Questions

### Why do I get ArgumentNullException?

You passed `null` as the shared secret. Provide a valid Base32 string.

### Why do I get ArgumentException?

You passed an empty string. Provide a non-empty Base32 string.

### Why do I get FormatException?

Your shared secret contains invalid Base32 characters. Valid characters: `A-Z`, `2-7`.

### The code doesn't work with Steam. Why?

Possible causes:
1. Wrong shared secret
2. System clock out of sync
3. Secret from different account
4. Steam's tolerance window (try adjacent time steps)

## Testing Questions

### How do I test with fixed time?

```csharp
var provider = new FixedTimeStepProvider(55752480); // Specific time step
var steamGuard = new SteamGuard(provider);
string code = steamGuard.GenerateAuthenticationCode(secret);
```

### How do I mock the time provider?

```csharp
var mock = new Mock<ITimeStepProvider>();
mock.Setup(p => p.GetCurrentTimeStep()).Returns(123456789L);
var steamGuard = new SteamGuard(mock.Object);
```

### Where are the test vectors?

See `SteamGuardTests.cs` in the test project. Known vectors from RFC 6238 and Steam community.

## Deployment Questions

### How do I install it?

```bash
dotnet add package SteamGuard.TOTP
```

### Is there a NuGet package?

Yes: `SteamGuard.TOTP` on NuGet.org

### Can I use it in .NET Framework?

No. Only .NET 10+ (net10.0).

### Does it work in Unity/Godot?

Only if they support .NET 10+. Check your engine's .NET version.

## Security Questions

### Is the shared secret exposed?

The library does not log, store, or transmit the secret. It's used only in-memory during code generation.

### Does it use secure random?

No random generation is used. TOTP is deterministic based on time and secret.

### Is it vulnerable to timing attacks?

The HMAC-SHA1 computation uses .NET's `HMACSHA1` which is constant-time. The truncation and mapping are also constant-time.

### Should I encrypt the secret at rest?

Yes. Store the shared secret securely (Key Vault, encrypted config, etc.). The library doesn't handle storage.

## Contribution Questions

### How do I contribute?

See [CONTRIBUTING.md](../CONTRIBUTING.md)

### Can I add SHA-256 support?

Not without breaking Steam compatibility. Steam uses HMAC-SHA1.

### Can I change the alphabet?

No. The Steam alphabet is fixed for compatibility.

## License Questions

### What license?

GPL-3.0-or-later (copyleft).

### Can I use it in commercial software?

Yes, but you must comply with GPL-3.0 (provide source, same license).

### Can I use it in closed-source software?

Only if you also make your software GPL-3.0-or-later compatible.

## Troubleshooting

### Codes don't match Steam app

1. Verify shared secret is correct
2. Sync system clock (NTP)
3. Try generating code for previous/next time step
4. Check for whitespace in secret

### Build fails

1. Ensure .NET 10 SDK installed
2. Run `dotnet restore`
3. Check NuciExtensions version (5.3.1)

### Tests fail

1. Run `dotnet test` from solution root
2. Check for clock skew in time-dependent tests
3. Use `FixedTimeStepProvider` for deterministic tests

## Still Have Questions?

- Open a GitHub Issue
- Check existing Issues and Discussions
- Review the [documentation](../docs/INDEX.md)