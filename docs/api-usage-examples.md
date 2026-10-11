# API Usage Examples

## Overview

This document provides practical examples for using the SteamGuard.TOTP library in various scenarios.

## Prerequisites

- .NET 10 SDK
- SteamGuard.TOTP NuGet package

## Installation

```bash
dotnet add package SteamGuard.TOTP
```

## Basic Usage

### Generate a Steam Guard Code

```csharp
using SteamGuard.TOTP;

// Create a time step provider (uses current system time)
var timeStepProvider = new TimeStepProvider();

// Create the SteamGuard instance
var steamGuard = new SteamGuard(timeStepProvider);

// Your Base32-encoded Steam shared secret
string sharedSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

// Generate the 5-character authentication code
string code = steamGuard.GenerateAuthenticationCode(sharedSecret);

Console.WriteLine($"Current Steam Guard code: {code}");
```

### Using the Default Constructor

```csharp
// SteamGuard has a parameterless constructor that uses TimeStepProvider internally
var steamGuard = new SteamGuard();
string code = steamGuard.GenerateAuthenticationCode(sharedSecret);
```

## Dependency Injection

### ASP.NET Core Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using SteamGuard.TOTP;

var builder = WebApplication.CreateBuilder(args);

// Register as singleton (stateless, thread-safe)
builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();

var app = builder.Build();
```

### Using in a Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using SteamGuard.TOTP;

[ApiController]
[Route("api/[controller]")]
public class SteamGuardController : ControllerBase
{
    private readonly ISteamGuard _steamGuard;

    public SteamGuardController(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard;
    }

    [HttpGet("code")]
    public IActionResult GetCode([FromQuery] string sharedSecret)
    {
        if (string.IsNullOrEmpty(sharedSecret))
            return BadRequest("Shared secret is required");

        string code = _steamGuard.GenerateAuthenticationCode(sharedSecret);
        return Ok(new { code });
    }
}
```

### Using in a Background Service

```csharp
using Microsoft.Extensions.Hosting;
using SteamGuard.TOTP;

public class SteamGuardBackgroundService : BackgroundService
{
    private readonly ISteamGuard _steamGuard;
    private readonly string _sharedSecret;

    public SteamGuardBackgroundService(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard;
        _sharedSecret = Environment.GetEnvironmentVariable("STEAM_SHARED_SECRET");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.Token.IsCancellationRequested)
        {
            string code = _steamGuard.GenerateAuthenticationCode(_sharedSecret);
            Console.WriteLine($"Current code: {code}");

            // Wait for the next 30-second interval
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

## Testing

### Unit Testing with Fixed Time Step

```csharp
using NUnit.Framework;
using SteamGuard.TOTP;
using SteamGuard.TOTP.UnitTests.Utils;

[TestFixture]
public class SteamGuardTests
{
    [Test]
    public void GenerateAuthenticationCode_KnownVector_ReturnsExpectedCode()
    {
        // Arrange
        var fixedProvider = new FixedTimeStepProvider(55752480); // 2023-01-01T12:00:00Z
        var steamGuard = new SteamGuard(fixedProvider);
        string sharedSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

        // Act
        string code = steamGuard.GenerateAuthenticationCode(sharedSecret);

        // Assert
        Assert.That(code, Is.EqualTo("R73NH"));
    }
}
```

### Mocking with Moq

```csharp
using Moq;
using NUnit.Framework;
using SteamGuard.TOTP;

[TestFixture]
public class SteamGuardMockTests
{
    [Test]
    public void GenerateAuthenticationCode_UsesInjectedTimeStep()
    {
        // Arrange
        var mockProvider = new Mock<ITimeStepProvider>();
        mockProvider.Setup(p => p.GetCurrentTimeStep()).Returns(123456789L);

        var steamGuard = new SteamGuard(mockProvider.Object);
        string sharedSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

        // Act
        string code = steamGuard.GenerateAuthenticationCode(sharedSecret);

        // Assert
        Assert.That(code, Is.EqualTo("EXPECTED_CODE"));
        mockProvider.Verify(p => p.GetCurrentTimeStep(), Times.Once);
    }
}
```

## Error Handling

### Null Shared Secret

```csharp
var steamGuard = new SteamGuard();

try
{
    string code = steamGuard.GenerateAuthenticationCode(null);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Empty Shared Secret

```csharp
var steamGuard = new SteamGuard();

try
{
    string code = steamGuard.GenerateAuthenticationCode("");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Invalid Base32 String

```csharp
var steamGuard = new SteamGuard();

try
{
    // Contains invalid Base32 characters
    string code = steamGuard.GenerateAuthenticationCode("INVALID!@#");
}
catch (FormatException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Complete Example: Steam Trade Offer Confirmation

```csharp
using SteamGuard.TOTP;

public class SteamTradeManager
{
    private readonly ISteamGuard _steamGuard;
    private readonly string _sharedSecret;

    public SteamTradeManager(ISteamGuard steamGuard, string sharedSecret)
    {
        _steamGuard = steamGuard;
        _sharedSecret = sharedSecret;
    }

    public string GetConfirmationCode()
    {
        return _steamGuard.GenerateAuthenticationCode(_sharedSecret);
    }

    public async Task ConfirmTradeOfferAsync(long tradeOfferId)
    {
        string code = GetConfirmationCode();

        // Use the code to confirm the trade offer via Steam Web API
        // (requires separate Steam Web API client)
        Console.WriteLine($"Confirming trade offer {tradeOfferId} with code: {code}");
    }
}
```

## Configuration

### Using appsettings.json

```json
{
  "SteamGuard": {
    "SharedSecret": "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"
  }
}
```

```csharp
public class SteamGuardOptions
{
    public string SharedSecret { get; set; }
}

// Registration
builder.Services.Configure<SteamGuardOptions>(
    builder.Configuration.GetSection("SteamGuard"));
builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();
```

## See Also

- [ISteamGuard Interface](api-reference/isteamguard.md)
- [ITimeStepProvider Interface](api-reference/itimestep-provider.md)
- [Quick Start Guide](quick-start.md)
- [Configuration](configuration.md)