# Configuration

## Overview

This document describes configuration options for the SteamGuard.TOTP library. The library is designed to be minimal and requires minimal configuration.

## Configuration Sources

### 1. Code-Based Configuration (Primary)

The library is configured entirely through code:

```csharp
using SteamGuard.TOTP;

// Default configuration (system time)
var steamGuard = new SteamGuard();

// Custom time provider (for testing or special requirements)
var steamGuard = new SteamGuard(new CustomTimeStepProvider());

// With dependency injection
services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
services.AddTransient<ISteamGuard, SteamGuard>();
```

### 2. Environment Variables

For the shared secret (not library configuration):

```bash
# Linux/macOS
export STEAM_SHARED_SECRET="GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"

# Windows PowerShell
$env:STEAM_SHARED_SECRET="GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"

# Windows CMD
set STEAM_SHARED_SECRET=GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ
```

```csharp
string secret = Environment.GetEnvironmentVariable("STEAM_SHARED_SECRET");
string code = steamGuard.GenerateAuthenticationCode(secret);
```

### 3. Configuration Files (ASP.NET Core)

```json
// appsettings.json
{
  "SteamGuard": {
    "SharedSecret": "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"
  }
}
```

```csharp
// Program.cs
builder.Services.Configure<SteamGuardOptions>(
    builder.Configuration.GetSection("SteamGuard"));

builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();
```

```csharp
// Options class
public class SteamGuardOptions
{
    public string SharedSecret { get; set; }
}

// Usage
public class SteamService
{
    private readonly ISteamGuard _steamGuard;
    private readonly string _sharedSecret;

    public SteamService(ISteamGuard steamGuard, IOptions<SteamGuardOptions> options)
    {
        _steamGuard = steamGuard;
        _sharedSecret = options.Value.SharedSecret;
    }

    public string GetCode() => _steamGuard.GenerateAuthenticationCode(_sharedSecret);
}
```

## Library Configuration Options

### TimeStepProvider

The only configurable aspect of the library is the time source:

| Provider | Use Case | Configuration |
|----------|----------|---------------|
| `TimeStepProvider` | Production (system time) | None |
| `FixedTimeStepProvider` | Testing (fixed time) | Constructor: `new FixedTimeStepProvider(timeStep)` |
| Custom `ITimeStepProvider` | Special requirements | Implement interface |

### No Other Configuration

The library has **no** configuration for:

- Algorithm (fixed: HMAC-SHA1)
- Time step interval (fixed: 30 seconds)
- Code length (fixed: 5 characters)
- Steam alphabet (fixed: `23456789BCDFGHJKMNPQRTVWXY`)
- Base32 variant (fixed: RFC 4648)
- Logging (none)
- Telemetry (none)
- Caching (none)
- Retry policies (none)
- Timeouts (none)

## Dependency Injection Configuration

### Minimal Registration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();

var app = builder.Build();
```

### With Options

```csharp
// Program.cs
builder.Services.Configure<SteamGuardOptions>(
    builder.Configuration.GetSection("SteamGuard"));

builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();

// Custom time provider
builder.Services.AddSingleton<ITimeStepProvider, CustomTimeStepProvider>();
```

### Scoped vs Singleton vs Transient

| Service | Lifetime | Reason |
|---------|----------|--------|
| `ITimeStepProvider` | Singleton | Stateless, thread-safe |
| `ISteamGuard` | Transient | Stateless, lightweight |
| `SteamGuardOptions` | Singleton | Configuration snapshot |

## Project Configuration

### SteamGuard.TOTP.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <PackageId>SteamGuard.TOTP</PackageId>
    <Version>1.0.0</Version>
    <Authors>Horatiu</Authors>
    <Description>Steam Guard TOTP code generator</Description>
    <PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/horatiu/steamguard.totp</PackageProjectUrl>
    <RepositoryUrl>https://github.com/horatiu/steamguard.totp</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>steam;totp;guard;authentication;2fa</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NuciExtensions" Version="5.3.1" />
  </ItemGroup>
</Project>
```

### SteamGuard.TOTP.UnitTests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="NUnit" Version="4.5.1" />
    <PackageReference Include="NUnit3TestAdapter" Version="6.2.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SteamGuard.TOTP\SteamGuard.TOTP.csproj" />
  </ItemGroup>
</Project>
```

## Build Configuration

### Debug vs Release

```bash
# Debug build (default)
dotnet build

# Release build
dotnet build --configuration Release

# Release with optimizations
dotnet build --configuration Release -p:Optimize=true
```

### Compiler Options

The project uses:
- `<ImplicitUsings>enable</ImplicitUsings>` - Global using directives
- `<Nullable>enable</Nullable>` - Nullable reference types
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` - XML docs

## Runtime Configuration

### No Runtime Config

The library reads no configuration at runtime. All behavior is determined by:
1. Constructor parameters
2. Method arguments
3. System time (via `TimeStepProvider`)

### Thread Safety

All configurations are thread-safe:
- `TimeStepProvider` - stateless
- `FixedTimeStepProvider` - immutable
- `SteamGuard` - stateless

## Security Configuration

### Secret Management

| Method | Security Level | Use Case |
|--------|----------------|----------|
| Environment variable | Medium | Development, containers |
| Azure Key Vault | High | Production Azure |
| HashiCorp Vault | High | Production multi-cloud |
| AWS Secrets Manager | High | Production AWS |
| User Secrets | Low | Local development only |

```csharp
// Azure Key Vault example
var client = new SecretClient(vaultUri, credential);
KeyVaultSecret secret = await client.GetSecretAsync("steam-shared-secret");
string code = steamGuard.GenerateAuthenticationCode(secret.Value);
```

## Validation

### Input Validation

The library validates:
- `sharedSecret` not null → `ArgumentNullException`
- `sharedSecret` not empty → `ArgumentException`
- `sharedSecret` valid Base32 → `FormatException`

### No Configuration Validation

Since there's no configuration object, there's no configuration validation.

## Related Documents

- [API Usage Examples](api-usage-examples.md)
- [Quick Start](quick-start.md)
- [Build and Deployment](build-and-deployment.md)
- [Dependencies](dependencies.md)