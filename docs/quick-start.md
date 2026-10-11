# Quick Start

## Overview

Get started with SteamGuard.TOTP in 5 minutes.

## Prerequisites

- .NET 10 SDK
- A Steam shared secret (Base32 encoded)

## Installation

```bash
dotnet add package SteamGuard.TOTP
```

## Basic Usage

```csharp
using SteamGuard.TOTP;

// Create generator
var steamGuard = new SteamGuard();

// Your Steam shared secret (from Steam mobile authenticator)
string sharedSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

// Generate code
string code = steamGuard.GenerateAuthenticationCode(sharedSecret);

Console.WriteLine($"Steam Guard Code: {code}");
// Output: Steam Guard Code: R73NH
```

## Dependency Injection (ASP.NET Core)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();

var app = builder.Build();

app.MapGet("/steam/code", (ISteamGuard steamGuard, string secret) =>
{
    return steamGuard.GenerateAuthenticationCode(secret);
});

app.Run();
```

## Testing

```csharp
// Unit test with fixed time
[Test]
public void TestCodeGeneration()
{
    var provider = new FixedTimeStepProvider(55752480); // 2023-01-01T12:00:00Z
    var steamGuard = new SteamGuard(provider);

    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");

    Assert.That(code, Is.EqualTo("R73NH"));
}
```

## Common Issues

| Issue | Solution |
|-------|----------|
| `ArgumentNullException` | Pass non-null secret |
| `ArgumentException` | Pass non-empty secret |
| `FormatException` | Ensure secret is valid Base32 (A-Z, 2-7) |
| Code doesn't work | Sync system clock (NTP) |

## Next Steps

- [API Usage Examples](api-usage-examples.md)
- [Configuration](configuration.md)
- [API Reference](api-reference/INDEX.md)