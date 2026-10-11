# Repository Structure

## Source Tree Layout

```
steamguard.totp/
├── .github/
│   └── workflows/
│       └── dotnet.yml              # CI/CD pipeline
├── docs/                           # Documentation (this directory)
│   ├── api-reference/
│   ├── behaviour/
│   ├── components/
│   ├── flows/
│   ├── INDEX.md
│   └── ... (other docs)
├── SteamGuard.TOTP/                # Main library project
│   ├── ISteamGuard.cs              # Public interface
│   ├── ITimeStepProvider.cs        # Time abstraction interface
│   ├── SteamGuard.cs               # Core implementation
│   ├── TimeStepProvider.cs         # Default time provider
│   ├── SteamGuard.TOTP.csproj      # Project file
│   ├── bin/                        # Build output (gitignored)
│   └── obj/                        # Build intermediates (gitignored)
├── SteamGuard.TOTP.UnitTests/      # Unit test project
│   ├── SteamGuardTests.cs          # Test cases
│   ├── SteamGuard.TOTP.UnitTests.csproj
│   ├── Utils/
│   │   └── FixedTimeStepProvider.cs # Test double
│   ├── bin/                        # Build output (gitignored)
│   └── obj/                        # Build intermediates (gitignored)
├── SteamGuard.TOTP.sln             # Solution file
├── ARCHITECTURE.md                 # Root architecture doc
├── CONTRIBUTING.md                 # Contribution guidelines
├── LICENSE                         # GPL-3.0-or-later
├── PRIVACY.md                      # Privacy policy
├── README.md                       # Project overview
├── SECURITY.md                     # Security policy
└── .gitignore                      # Git ignore rules
```

## Module Organisation

### SteamGuard.TOTP (Library)

| File | Purpose | Public/Internal |
|------|---------|-----------------|
| `ISteamGuard.cs` | Public contract for code generation | Public |
| `ITimeStepProvider.cs` | Time abstraction for testability | Public |
| `SteamGuard.cs` | TOTP algorithm implementation | Internal (concrete) |
| `TimeStepProvider.cs` | Default time-step implementation | Internal (concrete) |
| `SteamGuard.TOTP.csproj` | Project configuration, dependencies | Build |

**Namespace:** `SteamGuard.TOTP`

### SteamGuard.TOTP.UnitTests (Tests)

| File | Purpose |
|------|---------|
| `SteamGuardTests.cs` | NUnit test cases with Moq |
| `Utils/FixedTimeStepProvider.cs` | Deterministic test double |
| `SteamGuard.TOTP.UnitTests.csproj` | Test project config, references |

**Namespace:** `SteamGuard.TOTP.UnitTests`

## Project Configuration

### SteamGuard.TOTP.csproj

```xml
<TargetFramework>net10.0</TargetFramework>
<RootNamespace>SteamGuard.TOTP</RootNamespace>
<Version>1.1.1</Version>
<PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
<RepositoryUrl>https://github.com/hmlendea/steamguard.totp</RepositoryUrl>
```

**Dependencies:**
- `NuciExtensions` 5.3.1 (for `DateTimeExtensions.GetElapsedUnixTime`)

### SteamGuard.TOTP.UnitTests.csproj

```xml
<TargetFramework>net10.0</TargetFramework>
<IsPackable>false</IsPackable>
```

**Dependencies:**
- `Microsoft.NET.Test.Sdk` 18.3.0
- `Moq` 4.20.72
- `NUnit` 4.5.1
- `NUnit3TestAdapter` 6.2.0
- Project reference to `SteamGuard.TOTP`

## Build Output

- **NuGet package:** `SteamGuard.TOTP.{version}.nupkg` (from `dotnet pack`)
- **Assembly:** `SteamGuard.TOTP.dll`
- **Target:** `net10.0`

## CI/CD Pipeline

`.github/workflows/dotnet.yml`:
- Triggers: push to `master`, PR to `master`
- Jobs: Restore → Build → Test
- Runner: `ubuntu-latest`
- .NET version: `10.0.x`