# Logging

## Overview

This document describes the logging behavior of the SteamGuard.TOTP library.

## Library Logging

### No Built-in Logging

The SteamGuard.TOTP library **does not perform any logging**:

- No `ILogger` usage
- No `Console.WriteLine` calls
- No `Debug.WriteLine` calls
- No `Trace` calls
- No EventSource/EventListener
- No structured logging
- No log levels

### Rationale

| Reason | Explanation |
|--------|-------------|
| **Minimal dependency** | No logging abstraction dependency |
| **Performance** | Zero overhead |
| **Security** | No risk of logging secrets |
| **Simplicity** | Single responsibility: code generation |
| **Consumer control** | Consumers add logging as needed |

## Consumer Logging

### Recommended Patterns

#### 1. Decorator Pattern

```csharp
public class LoggingSteamGuard : ISteamGuard
{
    private readonly ISteamGuard _inner;
    private readonly ILogger<LoggingSteamGuard> _logger;

    public LoggingSteamGuard(ISteamGuard inner, ILogger<LoggingSteamGuard> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public string GenerateAuthenticationCode(string sharedSecret)
    {
        _logger.LogDebug("Generating Steam Guard code");
        
        try
        {
            string code = _inner.GenerateAuthenticationCode(sharedSecret);
            _logger.LogDebug("Code generated successfully");
            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Steam Guard code");
            throw;
        }
    }
}

// Registration
services.AddTransient<ISteamGuard>(sp => 
    new LoggingSteamGuard(
        new SteamGuard(sp.GetRequiredService<ITimeStepProvider>()),
        sp.GetRequiredService<ILogger<LoggingSteamGuard>>()));
```

#### 2. Middleware (ASP.NET Core)

```csharp
public class SteamGuardLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public SteamGuardLoggingMiddleware(RequestDelegate next, ILogger<SteamGuardLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/steam"))
        {
            _logger.LogInformation("Steam Guard API request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
        }
        
        await _next(context);
    }
}
```

#### 3. Extension Method

```csharp
public static class SteamGuardLoggingExtensions
{
    public static string GenerateWithLogging(this ISteamGuard steamGuard, 
        string sharedSecret, ILogger logger)
    {
        logger.LogDebug("Generating Steam Guard code");
        
        try
        {
            string code = steamGuard.GenerateAuthenticationCode(sharedSecret);
            logger.LogDebug("Code generated: {Code}", code);
            return code;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Code generation failed");
            throw;
        }
    }
}
```

## What to Log

### Safe to Log

| Information | Example |
|-------------|---------|
| Operation start/end | "Generating Steam Guard code" |
| Success/failure | "Code generated successfully" |
| Timing | "Code generation took 0.5ms" |
| Account identifier (not secret) | "Account: user123" |
| Error type | "FormatException: Invalid Base32" |

### Never Log

| Information | Reason |
|-------------|--------|
| Shared secret | Security - equivalent to password |
| Generated code | Security - valid for 30 seconds |
| Full stack traces in production | Information disclosure |

## Structured Logging

### Serilog Example

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

public class SerilogSteamGuard : ISteamGuard
{
    private readonly ISteamGuard _inner;

    public string GenerateAuthenticationCode(string sharedSecret)
    {
        using var activity = Log.Logger.BeginTimedOperation("SteamGuard.GenerateCode");
        
        try
        {
            string code = _inner.GenerateAuthenticationCode(sharedSecret);
            activity.Complete();
            return code;
        }
        catch (Exception ex)
        {
            activity.Fail(ex);
            throw;
        }
    }
}
```

### OpenTelemetry

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("SteamGuard.TOTP")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

// Custom activity source
public static class SteamGuardTelemetry
{
    private static readonly ActivitySource ActivitySource = new("SteamGuard.TOTP");

    public static string GenerateWithTelemetry(this ISteamGuard steamGuard, string secret)
    {
        using var activity = ActivitySource.StartActivity("GenerateAuthenticationCode");
        activity?.SetTag("steamguard.secret.length", secret?.Length ?? 0);
        
        try
        {
            string code = steamGuard.GenerateAuthenticationCode(secret);
            activity?.SetTag("steamguard.code.length", code.Length);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return code;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

## Log Levels

| Level | When to Use |
|-------|-------------|
| `Trace` | Detailed flow (time step, intermediate values) |
| `Debug` | Method entry/exit, parameters (sanitized) |
| `Information` | Successful code generation |
| `Warning` | Invalid input caught (FormatException) |
| `Error` | Unexpected exceptions |
| `Critical` | Never (library doesn't have critical failures) |

## Security Considerations

### Secret Masking

```csharp
public static class LogSanitizer
{
    public static string MaskSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret)) return "[empty]";
        if (secret.Length <= 4) return "[short]";
        return secret[..2] + new string('*', secret.Length - 4) + secret[^2..];
    }
}

// Usage
_logger.LogDebug("Generating code for secret: {Secret}", LogSanitizer.MaskSecret(secret));
```

### Code Masking

```csharp
_logger.LogDebug("Generated code: {Code}", code[..1] + "***" + code[^1..]);
```

## Testing Logging

### Verify No Library Logging

```csharp
[Test]
public void GenerateAuthenticationCode_DoesNotLog()
{
    // Arrange
    var logger = new TestLogger();
    var steamGuard = new SteamGuard();
    
    // Act
    steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");
    
    // Assert
    Assert.That(logger.Entries, Is.Empty);
}
```

### Test Consumer Logging

```csharp
[Test]
public void LoggingDecorator_LogsSuccess()
{
    // Arrange
    var mockGuard = new Mock<ISteamGuard>();
    mockGuard.Setup(g => g.GenerateAuthenticationCode(It.IsAny<string>()))
             .Returns("R73NH");
    
    var logger = new TestLogger();
    var decorator = new LoggingSteamGuard(mockGuard.Object, logger);
    
    // Act
    decorator.GenerateAuthenticationCode("SECRET");
    
    // Assert
    Assert.That(logger.Entries.Any(e => e.Contains("successfully")), Is.True);
}
```

## Configuration

### ASP.NET Core Logging Config

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SteamGuard": "Debug",
      "Microsoft": "Warning"
    }
  }
}
```

### Serilog Config

```json
// appsettings.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "SteamGuard": "Debug"
      }
    }
  }
}
```

## Related Documents

- [Error Handling](error-handling.md)
- [Security](security.md)
- [API Usage Examples](api-usage-examples.md)
- [Core Implementation](../components/core-implementation.md)