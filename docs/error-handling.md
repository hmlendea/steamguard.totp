# Error Handling

## Overview

This document describes the error handling strategy of the SteamGuard.TOTP library, including exception types, error conditions, and best practices for consumers.

## Exception Types

### Public Exceptions

| Exception | Thrown By | Condition |
|-----------|-----------|-----------|
| `ArgumentNullException` | `SteamGuard.GenerateAuthenticationCode` | `sharedSecret` is `null` |
| `ArgumentException` | `SteamGuard.GenerateAuthenticationCode` | `sharedSecret` is empty |
| `FormatException` | `SteamGuard.GenerateAuthenticationCode` | `sharedSecret` contains invalid Base32 characters |

### Internal Exceptions

Internal methods may throw:
- `CryptographicException` - HMAC computation failure (extremely rare)
- `IndexOutOfRangeException` - Should never occur (guarded by validation)

## Error Conditions

### 1. Null Shared Secret

```csharp
var steamGuard = new SteamGuard();

try
{
    string code = steamGuard.GenerateAuthenticationCode(null);
}
catch (ArgumentNullException ex)
{
    // ex.Message: "Shared secret cannot be null. (Parameter 'sharedSecret')"
    // ex.ParamName: "sharedSecret"
    Console.WriteLine($"Parameter error: {ex.ParamName}");
}
```

### 2. Empty Shared Secret

```csharp
try
{
    string code = steamGuard.GenerateAuthenticationCode("");
}
catch (ArgumentException ex)
{
    // ex.Message: "Shared secret cannot be empty. (Parameter 'sharedSecret')"
    // ex.ParamName: "sharedSecret"
    Console.WriteLine($"Invalid argument: {ex.Message}");
}
```

### 3. Invalid Base32 Characters

```csharp
try
{
    // Contains invalid characters: !, @, #, $, %, ^, &, *, (, )
    string code = steamGuard.GenerateAuthenticationCode("INVALID!@#$%^&*()");
}
catch (FormatException ex)
{
    // ex.Message: "Invalid Base32 string: contains invalid characters."
    Console.WriteLine($"Format error: {ex.Message}");
}
```

### 4. Whitespace-Only Secret

```csharp
try
{
    string code = steamGuard.GenerateAuthenticationCode("   ");
}
catch (ArgumentException ex)
{
    // Treated as empty after trimming
    Console.WriteLine($"Invalid argument: {ex.Message}");
}
```

## Error Handling Patterns

### Defensive Programming

```csharp
public class SteamGuardService
{
    private readonly ISteamGuard _steamGuard;

    public SteamGuardService(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard ?? throw new ArgumentNullException(nameof(steamGuard));
    }

    public string GenerateCode(string sharedSecret)
    {
        if (string.IsNullOrWhiteSpace(sharedSecret))
            throw new ArgumentException("Shared secret is required", nameof(sharedSecret));

        try
        {
            return _steamGuard.GenerateAuthenticationCode(sharedSecret);
        }
        catch (FormatException ex)
        {
            // Log and rethrow with context
            throw new InvalidOperationException(
                "Failed to generate Steam Guard code. The shared secret may be invalid.",
                ex);
        }
    }
}
```

### Graceful Degradation

```csharp
public class ResilientSteamGuardService
{
    private readonly ISteamGuard _steamGuard;
    private readonly ILogger _logger;

    public string TryGenerateCode(string sharedSecret, int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return _steamGuard.GenerateAuthenticationCode(sharedSecret);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Shared secret is null");
                return null; // Cannot recover
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Shared secret is empty");
                return null; // Cannot recover
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Invalid Base32 format, attempt {Attempt}", attempt + 1);
                if (attempt == maxRetries - 1)
                    return null; // Exhausted retries
            }
        }

        return null;
    }
}
```

### Validation Before Generation

```csharp
public static class SteamGuardValidator
{
    private static readonly HashSet<char> ValidBase32Chars = 
        new("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567", StringComparer.OrdinalIgnoreCase);

    public static bool IsValidSharedSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret))
            return false;

        return secret.All(c => ValidBase32Chars.Contains(char.ToUpperInvariant(c)));
    }

    public static string GenerateCodeSafely(ISteamGuard steamGuard, string secret)
    {
        if (!IsValidSharedSecret(secret))
            throw new ArgumentException("Invalid shared secret format", nameof(secret));

        return steamGuard.GenerateAuthenticationCode(secret);
    }
}
```

## Exception Hierarchy

```
System.Exception
├── System.SystemException
│   ├── System.ArgumentException
│   │   └── System.ArgumentNullException
│   └── System.FormatException
└── System.Security.Cryptography.CryptographicException (internal)
```

## Error Recovery

### Non-Recoverable Errors

| Error | Recovery |
|-------|----------|
| `ArgumentNullException` | Fix caller code - provide non-null secret |
| `ArgumentException` (empty) | Fix caller code - provide non-empty secret |
| `FormatException` (invalid chars) | Fix caller code - provide valid Base32 |

### Recoverable Errors

| Error | Recovery |
|-------|----------|
| `CryptographicException` | Retry (extremely rare) |
| Time step mismatch | Regenerate code (wait for next 30s window) |

## Logging

### No Built-in Logging

The library does **not** log errors. Consumers should implement their own logging:

```csharp
public class LoggingSteamGuardDecorator : ISteamGuard
{
    private readonly ISteamGuard _inner;
    private readonly ILogger _logger;

    public string GenerateAuthenticationCode(string sharedSecret)
    {
        try
        {
            _logger.LogDebug("Generating Steam Guard code");
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
```

## Testing Error Conditions

### Unit Tests for Errors

```csharp
[Test]
public void GenerateAuthenticationCode_NullSecret_ThrowsArgumentNullException()
{
    var steamGuard = new SteamGuard();
    
    var ex = Assert.Throws<ArgumentNullException>(
        () => steamGuard.GenerateAuthenticationCode(null));
    
    Assert.That(ex.ParamName, Is.EqualTo("sharedSecret"));
}

[Test]
public void GenerateAuthenticationCode_EmptySecret_ThrowsArgumentException()
{
    var steamGuard = new SteamGuard();
    
    var ex = Assert.Throws<ArgumentException>(
        () => steamGuard.GenerateAuthenticationCode(""));
    
    Assert.That(ex.Message, Does.Contain("empty"));
}

[Test]
public void GenerateAuthenticationCode_InvalidBase32_ThrowsFormatException()
{
    var steamGuard = new SteamGuard();
    
    var ex = Assert.Throws<FormatException>(
        () => steamGuard.GenerateAuthenticationCode("INVALID!@#"));
    
    Assert.That(ex.Message, Does.Contain("Base32"));
}
```

## Best Practices

### 1. Validate Input Early

```csharp
// Good: Validate before calling library
if (string.IsNullOrEmpty(sharedSecret))
{
    throw new ArgumentException("Shared secret is required", nameof(sharedSecret));
}

string code = steamGuard.GenerateAuthenticationCode(sharedSecret);
```

### 2. Catch Specific Exceptions

```csharp
// Good: Catch specific exceptions
try
{
    string code = steamGuard.GenerateAuthenticationCode(secret);
}
catch (ArgumentNullException)
{
    // Handle null input
}
catch (ArgumentException)
{
    // Handle empty input
}
catch (FormatException)
{
    // Handle invalid format
}
```

### 3. Don't Catch and Swallow

```csharp
// Bad: Swallowing exceptions
try
{
    string code = steamGuard.GenerateAuthenticationCode(secret);
}
catch (Exception)
{
    return "00000"; // Wrong!
}
```

### 4. Provide Context in Wrappers

```csharp
// Good: Add context when wrapping
try
{
    return steamGuard.GenerateAuthenticationCode(secret);
}
catch (FormatException ex)
{
    throw new SteamGuardException(
        $"Failed to generate code for account '{accountName}'. The shared secret may be corrupted.",
        ex);
}
```

## Error Message Reference

| Exception | Message Pattern |
|-----------|-----------------|
| `ArgumentNullException` | `Shared secret cannot be null. (Parameter 'sharedSecret')` |
| `ArgumentException` | `Shared secret cannot be empty. (Parameter 'sharedSecret')` |
| `FormatException` | `Invalid Base32 string: contains invalid characters.` |

## Related Documents

- [Core Implementation](../components/core-implementation.md)
- [Code Generation Flow](../flows/code-generation.md)
- [API Usage Examples](api-usage-examples.md)
- [Testing](testing.md)