# Inspect, Edit, Delete Behaviour

## Overview

This document describes the inspect, edit, and delete capabilities for the SteamGuard.TOTP library. Since this is a stateless, pure computation library with no persistent storage, these operations map to code inspection, configuration, and dependency management rather than data manipulation.

## Inspect

### Inspecting Generated Codes

```csharp
using SteamGuard.TOTP;

var steamGuard = new SteamGuard();
string sharedSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

// Generate and inspect current code
string code = steamGuard.GenerateAuthenticationCode(sharedSecret);
Console.WriteLine($"Code: {code}"); // e.g., "R73NH"
Console.WriteLine($"Length: {code.Length}"); // Always 5
Console.WriteLine($"Valid chars: {string.Join("", code.Distinct().OrderBy(c => c))}"); // Subset of 23456789BCDFGHJKMNPQRTVWXY
```

### Inspecting Time Steps

```csharp
using SteamGuard.TOTP;

var provider = new TimeStepProvider();
long timeStep = provider.GetCurrentTimeStep();

Console.WriteLine($"Current time step: {timeStep}");
Console.WriteLine($"Unix time: {timeStep * 30}");
Console.WriteLine($"Next change in: {30 - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 30)} seconds");
```

### Inspecting Internal State (Debugging)

```csharp
using SteamGuard.TOTP;
using System.Security.Cryptography;

var steamGuard = new SteamGuard();
string sharedSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

// The library doesn't expose internal state, but you can trace:
// 1. Base32 decode
// 2. Time step calculation
// 3. HMAC-SHA1 computation
// 4. Dynamic truncation
// 5. Steam alphabet mapping

// For debugging, use FixedTimeStepProvider to freeze time
var fixedProvider = new FixedTimeStepProvider(55752480);
var debugGuard = new SteamGuard(fixedProvider);
string code = debugGuard.GenerateAuthenticationCode(sharedSecret);
// Code will always be "R73NH" for this time step
```

### Inspecting Dependencies

```bash
# View project dependencies
dotnet list SteamGuard.TOTP/SteamGuard.TOTP.csproj package

# View transitive dependencies
dotnet list SteamGuard.TOTP/SteamGuard.TOTP.csproj package --include-transitive

# View vulnerable packages
dotnet list SteamGuard.TOTP/SteamGuard.TOTP.csproj package --vulnerable
```

## Edit

### Editing Configuration

The library has no configuration files. Behavior is controlled through:

#### 1. Constructor Injection (Time Source)

```csharp
// Default: system time
var steamGuard = new SteamGuard();

// Custom: fixed time for testing
var steamGuard = new SteamGuard(new FixedTimeStepProvider(123456789L));

// Custom: mock time provider
var steamGuard = new SteamGuard(mockTimeProvider.Object);
```

#### 2. Dependency Injection Registration

```csharp
// Program.cs
builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();

// Or with custom implementation
builder.Services.AddSingleton<ITimeStepProvider, CustomTimeStepProvider>();
```

#### 3. Shared Secret Management

```csharp
// From environment variable
string secret = Environment.GetEnvironmentVariable("STEAM_SHARED_SECRET");

// From configuration
string secret = configuration["SteamGuard:SharedSecret"];

// From secure vault (Azure Key Vault, HashiCorp Vault, etc.)
string secret = await secretClient.GetSecretAsync("steam-shared-secret");
```

### Editing Project Files

#### Adding the Package

```xml
<!-- SteamGuard.TOTP.csproj -->
<PackageReference Include="SteamGuard.TOTP" Version="1.0.0" />
```

#### Updating Dependencies

```bash
# Update NuciExtensions
dotnet add SteamGuard.TOTP/SteamGuard.TOTP.csproj package NuciExtensions --version 5.3.2

# Update test packages
dotnet add SteamGuard.TOTP.UnitTests/SteamGuard.TOTP.UnitTests.csproj package NUnit --version 4.6.0
```

#### Changing Target Framework

```xml
<!-- SteamGuard.TOTP.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- Change to net9.0, net8.0, etc. if needed -->
</PropertyGroup>
```

## Delete

### Removing the Library

```bash
# Remove package reference
dotnet remove SteamGuard.TOTP/SteamGuard.TOTP.csproj package SteamGuard.TOTP

# Or delete the project reference
dotnet remove MyProject/MyProject.csproj reference SteamGuard.TOTP/SteamGuard.TOTP.csproj
```

### Removing Generated Codes

Since the library generates codes on-demand with no persistence:

```csharp
// No deletion needed - codes are ephemeral
string code = steamGuard.GenerateAuthenticationCode(secret);
// Code exists only in memory, garbage collected after use
```

### Removing Test Artifacts

```bash
# Clean test output
dotnet clean SteamGuard.TOTP.UnitTests/SteamGuard.TOTP.UnitTests.csproj

# Remove bin/obj folders
rm -rf SteamGuard.TOTP/bin SteamGuard.TOTP/obj
rm -rf SteamGuard.TOTP.UnitTests/bin SteamGuard.TOTP.UnitTests/obj
```

### Removing from DI Container

```csharp
// Simply don't register the services
// Or remove registrations:
services.RemoveAll<ISteamGuard>();
services.RemoveAll<ITimeStepProvider>();
```

## State Management

### No Persistent State

The library maintains **no persistent state**:

| Aspect | Behavior |
|--------|----------|
| Database | None |
| File storage | None |
| Cache | None |
| Session | None |
| Telemetry | None |
| Logging | None |

### Ephemeral Computation

Each call to `GenerateAuthenticationCode`:
1. Takes input (shared secret)
2. Computes time step
3. Computes HMAC-SHA1
4. Truncates and maps to Steam alphabet
5. Returns 5-character string
6. **No side effects**

### Thread Safety

- `SteamGuard` is stateless and thread-safe
- `TimeStepProvider` is stateless and thread-safe
- `FixedTimeStepProvider` is immutable and thread-safe
- Multiple threads can call `GenerateAuthenticationCode` concurrently

## Cleanup

### Resource Disposal

The library implements no `IDisposable` and holds no unmanaged resources:

```csharp
// No using statement needed
var steamGuard = new SteamGuard();
string code = steamGuard.GenerateAuthenticationCode(secret);
// No cleanup required
```

### Memory Management

- No large object allocations
- No pinned memory
- No finalizers
- Standard .NET garbage collection applies

## Related Documents

- [Browse and Search](behaviour/browse-and-search.md)
- [API Usage Examples](api-usage-examples.md)
- [Core Implementation](../components/core-implementation.md)
- [Code Generation Flow](../flows/code-generation.md)
- [Testing](testing.md)