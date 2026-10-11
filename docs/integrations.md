# Integrations

## Overview

This document describes how to integrate SteamGuard.TOTP with various platforms, frameworks, and tools.

## .NET Integrations

### ASP.NET Core

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

### Minimal API

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
builder.Services.AddTransient<ISteamGuard, SteamGuard>();

var app = builder.Build();

app.MapPost("/api/steam/code", (SteamCodeRequest request, ISteamGuard steamGuard) =>
{
    if (string.IsNullOrEmpty(request.SharedSecret))
        return Results.BadRequest("Shared secret required");

    try
    {
        string code = steamGuard.GenerateAuthenticationCode(request.SharedSecret);
        return Results.Ok(new { code });
    }
    catch (FormatException)
    {
        return Results.BadRequest("Invalid shared secret format");
    }
});

app.Run();

record SteamCodeRequest(string SharedSecret);
```

### Blazor

```razor
@page "/steam-guard"
@inject ISteamGuard SteamGuard

<h3>Steam Guard Code Generator</h3>

<input @bind="sharedSecret" placeholder="Enter shared secret" />
<button @onclick="GenerateCode">Generate</button>

@if (!string.IsNullOrEmpty(code))
{
    <div class="code">@code</div>
}

@code {
    private string sharedSecret = "";
    private string code = "";

    private void GenerateCode()
    {
        try
        {
            code = SteamGuard.GenerateAuthenticationCode(sharedSecret);
        }
        catch (Exception ex)
        {
            code = $"Error: {ex.Message}";
        }
    }
}
```

### WPF / WinUI / MAUI

```csharp
// ViewModel
public class SteamGuardViewModel : INotifyPropertyChanged
{
    private readonly ISteamGuard _steamGuard;
    private string _sharedSecret;
    private string _code;

    public string SharedSecret
    {
        get => _sharedSecret;
        set { _sharedSecret = value; OnPropertyChanged(); }
    }

    public string Code
    {
        get => _code;
        set { _code = value; OnPropertyChanged(); }
    }

    public ICommand GenerateCommand { get; }

    public SteamGuardViewModel(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard;
        GenerateCommand = new RelayCommand(Generate);
    }

    private void Generate()
    {
        try
        {
            Code = _steamGuard.GenerateAuthenticationCode(SharedSecret);
        }
        catch (Exception ex)
        {
            Code = $"Error: {ex.Message}";
        }
    }

    // INotifyPropertyChanged implementation...
}
```

## Container Integrations

### Docker

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SteamGuard.TOTP/SteamGuard.TOTP.csproj", "SteamGuard.TOTP/"]
COPY ["MyApp/MyApp.csproj", "MyApp/"]
RUN dotnet restore "MyApp/MyApp.csproj"
COPY . .
WORKDIR "/src/MyApp"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Kubernetes

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: steam-guard-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: steam-guard-service
  template:
    metadata:
      labels:
        app: steam-guard-service
    spec:
      containers:
      - name: steam-guard-service
        image: myregistry/steam-guard-service:latest
        ports:
        - containerPort: 8080
        env:
        - name: STEAM_SHARED_SECRET
          valueFrom:
            secretKeyRef:
              name: steam-secrets
              key: shared-secret
---
# secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: steam-secrets
type: Opaque
stringData:
  shared-secret: "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"
```

## Cloud Integrations

### Azure Functions

```csharp
// Function.cs
public class SteamGuardFunction
{
    private readonly ISteamGuard _steamGuard;

    public SteamGuardFunction(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard;
    }

    [FunctionName("GenerateSteamCode")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "steam/code")] HttpRequest req,
        ILogger log)
    {
        string secret = req.Query["secret"];

        if (string.IsNullOrEmpty(secret))
            return new BadRequestObjectResult("Secret required");

        try
        {
            string code = _steamGuard.GenerateAuthenticationCode(secret);
            return new OkObjectResult(new { code });
        }
        catch (FormatException)
        {
            return new BadRequestObjectResult("Invalid secret format");
        }
    }
}

// Startup.cs
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ITimeStepProvider, TimeStepProvider>();
        builder.Services.AddTransient<ISteamGuard, SteamGuard>();
    }
}
```

### AWS Lambda

```csharp
// Function.cs
public class SteamGuardFunction
{
    private readonly ISteamGuard _steamGuard;

    public SteamGuardFunction()
    {
        var provider = new TimeStepProvider();
        _steamGuard = new SteamGuard(provider);
    }

    public APIGatewayProxyResponse GenerateCode(APIGatewayProxyRequest request, ILambdaContext context)
    {
        string secret = request.QueryStringParameters?.GetValueOrDefault("secret");

        if (string.IsNullOrEmpty(secret))
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Secret required" };

        try
        {
            string code = _steamGuard.GenerateAuthenticationCode(secret);
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new { code })
            };
        }
        catch (FormatException)
        {
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Invalid secret format" };
        }
    }
}
```

### Google Cloud Functions

```csharp
// Function.cs
public class SteamGuardFunction : IHttpFunction
{
    private readonly ISteamGuard _steamGuard;

    public SteamGuardFunction()
    {
        var provider = new TimeStepProvider();
        _steamGuard = new SteamGuard(provider);
    }

    public async Task HandleAsync(HttpContext context)
    {
        string secret = context.Request.Query["secret"];

        if (string.IsNullOrEmpty(secret))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Secret required");
            return;
        }

        try
        {
            string code = _steamGuard.GenerateAuthenticationCode(secret);
            await context.Response.WriteAsJsonAsync(new { code });
        }
        catch (FormatException)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid secret format");
        }
    }
}
```

## CI/CD Integrations

### GitHub Actions

```yaml
# .github/workflows/steam-trade.yml
name: Steam Trade Bot

on:
  schedule:
    - cron: '*/5 * * * *'  # Every 5 minutes

jobs:
  check-trades:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Build
      run: dotnet build SteamGuard.TOTP.sln -c Release

    - name: Generate Code
      run: |
        dotnet run --project SteamTradeBot/SteamTradeBot.csproj \
          --secret "${{ secrets.STEAM_SHARED_SECRET }}"
      env:
        STEAM_SHARED_SECRET: ${{ secrets.STEAM_SHARED_SECRET }}
```

### GitLab CI

```yaml
# .gitlab-ci.yml
stages:
  - build
  - test
  - deploy

build:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet restore SteamGuard.TOTP.sln
    - dotnet build SteamGuard.TOTP.sln -c Release

test:
  stage: test
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet test SteamGuard.TOTP.sln -c Release --no-build

deploy:
  stage: deploy
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet pack SteamGuard.TOTP/SteamGuard.TOTP.csproj -c Release -o ./nupkg
    - dotnet nuget push ./nupkg/*.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
  only:
    - tags
```

## Third-Party Library Integrations

### Steam Web API (SteamKit2)

```csharp
// Using SteamKit2 for trade confirmations
using SteamKit2;
using SteamGuard.TOTP;

public class SteamTradeBot
{
    private readonly SteamGuard _steamGuard;
    private readonly SteamUser _steamUser;

    public SteamTradeBot(string sharedSecret)
    {
        _steamGuard = new SteamGuard();
        _sharedSecret = sharedSecret;
    }

    public async Task ConfirmTradeOfferAsync(TradeOffer offer)
    {
        string code = _steamGuard.GenerateAuthenticationCode(_sharedSecret);

        // Use code with SteamKit2 to confirm trade
        await _steamUser.AcceptTradeOffer(offer.TradeOfferId, code);
    }
}
```

### IdentityServer / OpenID Connect

```csharp
// Custom TOTP validator for IdentityServer
public class SteamTotpValidator : ITokenValidator
{
    private readonly ISteamGuard _steamGuard;

    public SteamTotpValidator(ISteamGuard steamGuard)
    {
        _steamGuard = steamGuard;
    }

    public bool Validate(string sharedSecret, string providedCode)
    {
        string expectedCode = _steamGuard.GenerateAuthenticationCode(sharedSecret);
        return expectedCode == providedCode;
    }
}
```

## Language Bindings

### PowerShell

```powershell
# SteamGuard.ps1
Add-Type -Path "SteamGuard.TOTP.dll"

$steamGuard = New-Object SteamGuard.TOTP.SteamGuard
$secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"
$code = $steamGuard.GenerateAuthenticationCode($secret)
Write-Host "Steam Guard Code: $code"
```

### Python (via pythonnet)

```python
# steam_guard.py
import clr
clr.AddReference("SteamGuard.TOTP")
from SteamGuard.TOTP import SteamGuard

steam_guard = SteamGuard()
secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ"
code = steam_guard.GenerateAuthenticationCode(secret)
print(f"Steam Guard Code: {code}")
```

## IDE Integrations

### Visual Studio Code Snippets

```json
// steamguard.code-snippets
{
  "Steam Guard Code Generation": {
    "prefix": "steamguard",
    "body": [
      "using SteamGuard.TOTP;",
      "",
      "var steamGuard = new SteamGuard();",
      "string code = steamGuard.GenerateAuthenticationCode(\"${1:sharedSecret}\");"
    ],
    "description": "Generate Steam Guard authentication code"
  }
}
```

### Rider / Visual Studio Live Templates

```xml
<!-- Live template for SteamGuard -->
<template name="steamguard" value="using SteamGuard.TOTP;&#xA;&#xA;var steamGuard = new SteamGuard();&#xA;string code = steamGuard.GenerateAuthenticationCode(\"$SECRET$\");" />
```

## Related Documents

- [API Usage Examples](api-usage-examples.md)
- [Configuration](configuration.md)
- [Build and Deployment](build-and-deployment.md)
- [Quick Start](quick-start.md)