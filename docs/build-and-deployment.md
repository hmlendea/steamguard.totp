# Build and Deployment

## Overview

This document describes the build process, packaging, and deployment for the SteamGuard.TOTP library.

## Prerequisites

### Required Software

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0+ | Build, test, pack |
| Git | 2.40+ | Version control |
| NuGet CLI | 6.0+ | Package management (optional) |

### Verify Installation

```bash
dotnet --version
# Should output 10.0.x or later

git --version
# Should output 2.40.x or later
```

## Building

### Restore Dependencies

```bash
# Restore all projects
dotnet restore SteamGuard.TOTP.sln

# Or restore specific project
dotnet restore SteamGuard.TOTP/SteamGuard.TOTP.csproj
```

### Build Solution

```bash
# Build all configurations
dotnet build SteamGuard.TOTP.sln --configuration Release

# Build specific project
dotnet build SteamGuard.TOTP/SteamGuard.TOTP.csproj --configuration Release --no-restore
```

### Build Output

```
SteamGuard.TOTP/bin/Release/net10.0/
├── SteamGuard.TOTP.dll
├── SteamGuard.TOTP.pdb
├── SteamGuard.TOTP.deps.json
└── SteamGuard.TOTP.xml (documentation)
```

## Testing

### Run All Tests

```bash
# Run tests with default settings
dotnet test SteamGuard.TOTP.sln --configuration Release --no-build

# Run with detailed output
dotnet test SteamGuard.TOTP.sln --configuration Release --no-build --logger "console;verbosity=detailed"

# Run with TRX output
dotnet test SteamGuard.TOTP.sln --configuration Release --no-build --logger "trx;LogFileName=test_results.trx"
```

### Run Specific Tests

```bash
# Run specific test class
dotnet test SteamGuard.TOTP.UnitTests/SteamGuard.TOTP.UnitTests.csproj --filter "FullyQualifiedName~SteamGuardTests"

# Run specific test method
dotnet test SteamGuard.TOTP.UnitTests/SteamGuard.TOTP.UnitTests.csproj --filter "FullyQualifiedName~GenerateAuthenticationCode_KnownVector"
```

### Test Coverage

```bash
# Install coverlet if not present
dotnet tool install --global coverlet.console

# Run with coverage
dotnet test SteamGuard.TOTP.UnitTests/SteamGuard.TOTP.UnitTests.csproj \
  --configuration Release \
  --no-build \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage.xml
```

## Packaging

### Create NuGet Package

```bash
# Pack the library project
dotnet pack SteamGuard.TOTP/SteamGuard.TOTP.csproj \
  --configuration Release \
  --no-build \
  --output ./nupkg

# Output: ./nupkg/SteamGuard.TOTP.1.0.0.nupkg
```

### Package Contents

```
SteamGuard.TOTP.1.0.0.nupkg
├── lib/net10.0/
│   ├── SteamGuard.TOTP.dll
│   └── SteamGuard.TOTP.xml
├── SteamGuard.TOTP.nuspec
└── [content files]
```

### Package Metadata (from csproj)

```xml
<PropertyGroup>
  <PackageId>SteamGuard.TOTP</PackageId>
  <Version>1.0.0</Version>
  <Authors>Horatiu</Authors>
  <Description>Steam Guard TOTP code generator</Description>
  <PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/horatiu/steamguard.totp</PackageProjectUrl>
  <RepositoryUrl>https://github.com/horatiu/steamguard.totp</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageTags>steam;totp;guard;authentication;2fa</PackageTags>
  <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

## Versioning

### Version Scheme

| Component | Format | Example |
|-----------|--------|---------|
| Major | Breaking changes | 1 → 2 |
| Minor | New features (non-breaking) | 1.0 → 1.1 |
| Patch | Bug fixes | 1.0.0 → 1.0.1 |
| Pre-release | Alpha/beta/rc | 1.0.0-alpha.1 |

### Setting Version

```bash
# Via command line
dotnet pack SteamGuard.TOTP/SteamGuard.TOTP.csproj -p:Version=1.2.3

# Or update csproj
<Version>1.2.3</Version>
```

### Git Tagging

```bash
# Tag release
git tag -a v1.2.3 -m "Release 1.2.3"
git push origin v1.2.3
```

## Publishing

### Publish to NuGet.org

```bash
# Get API key from nuget.org
# Set as environment variable or use --api-key

dotnet nuget push ./nupkg/SteamGuard.TOTP.1.0.0.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key $NUGET_API_KEY
```

### Publish to GitHub Packages

```bash
dotnet nuget push ./nupkg/SteamGuard.TOTP.1.0.0.nupkg \
  --source "github" \
  --api-key $GITHUB_TOKEN
```

### Publish Symbols

```bash
# Symbols are included in .snupkg
dotnet nuget push ./nupkg/SteamGuard.TOTP.1.0.0.snupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key $NUGET_API_KEY
```

## CI/CD Pipeline

### GitHub Actions Workflow (`.github/workflows/dotnet.yml`)

```yaml
name: .NET

on:
  push:
    branches: [master]
  pull_request:
    branches: [master]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore
      run: dotnet restore SteamGuard.TOTP.sln
    
    - name: Build
      run: dotnet build SteamGuard.TOTP.sln --configuration Release --no-restore
    
    - name: Test
      run: dotnet test SteamGuard.TOTP.sln --configuration Release --no-build --logger "trx"
    
    - name: Pack
      run: dotnet pack SteamGuard.TOTP/SteamGuard.TOTP.csproj --configuration Release --no-build --output ./nupkg
    
    - name: Upload Artifacts
      uses: actions/upload-artifact@v4
      with:
        name: nupkg
        path: ./nupkg/*.nupkg
```

### Release Workflow (`.github/workflows/github-release.yml`)

```yaml
name: GitHub Release

on:
  push:
    tags:
      - 'v*'

jobs:
  release:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore
      run: dotnet restore SteamGuard.TOTP.sln
    
    - name: Build
      run: dotnet build SteamGuard.TOTP.sln --configuration Release --no-restore
    
    - name: Test
      run: dotnet test SteamGuard.TOTP.sln --configuration Release --no-build
    
    - name: Pack
      run: dotnet pack SteamGuard.TOTP/SteamGuard.TOTP.csproj --configuration Release --no-build --output ./nupkg
    
    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: ./nupkg/*.nupkg
        generate_release_notes: true
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## Deployment Targets

### NuGet Package Consumers

```bash
# Install in consumer project
dotnet add package SteamGuard.TOTP --version 1.0.0
```

### GitHub Releases

- Binary assets attached to GitHub Releases
- Source code archives (automatic)
- Release notes from commit history

### Local Development

```bash
# Reference local package
dotnet add package SteamGuard.TOTP --source ./nupkg

# Or project reference
dotnet add reference ../SteamGuard.TOTP/SteamGuard.TOTP.csproj
```

## Quality Gates

### Pre-Release Checklist

- [ ] All tests pass (`dotnet test`)
- [ ] No compiler warnings (`dotnet build -warnaserror`)
- [ ] Package validates (`dotnet nuget verify`)
- [ ] Documentation updated
- [ ] Changelog updated
- [ ] Version bumped
- [ ] Git tag created

### Automated Checks

| Check | Tool | Command |
|-------|------|---------|
| Compile | `dotnet build` | `dotnet build -warnaserror` |
| Test | `dotnet test` | `dotnet test --no-build` |
| Static Analysis | .NET Analyzers | `dotnet build /p:EnableNETAnalyzers=true` |
| Security | `dotnet list package --vulnerable` | `dotnet list package --vulnerable` |
| Format | `dotnet format` | `dotnet format --verify-no-changes` |

## Troubleshooting

### Common Build Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| `NETSDK1045` | Wrong .NET version | Install .NET 10 SDK |
| `NU1101` | Package not found | Run `dotnet restore` |
| `CS0246` | Missing namespace | Check `using SteamGuard.TOTP;` |
| Test discovery fails | Missing adapter | Ensure `NUnit3TestAdapter` referenced |

### Package Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| `NU5048` | Missing license | Add `PackageLicenseExpression` |
| `NU5128` | No assembly in lib | Check `dotnet pack` output |
| Symbol mismatch | PDB not included | Set `IncludeSymbols=true` |

## Related Documents

- [Repository Structure](repository-structure.md)
- [Dependencies](dependencies.md)
- [CI/CD Configuration](.github/workflows/dotnet.yml)
- [Contributing](../CONTRIBUTING.md)