# Change Guide

## Overview

This document provides guidelines for making changes to the SteamGuard.TOTP library, including versioning, breaking changes, and migration paths.

## Versioning Policy

### Semantic Versioning

This project follows [Semantic Versioning 2.0.0](https://semver.org/):

| Version | When to Increment |
|---------|-------------------|
| **Major** (X.0.0) | Breaking changes to public API |
| **Minor** (X.Y.0) | New features, non-breaking |
| **Patch** (X.Y.Z) | Bug fixes, non-breaking |

### Current Version

| Component | Version |
|-----------|---------|
| Library | 1.0.0 |
| .NET Target | net10.0 |
| NuciExtensions | 5.3.1 |

## Types of Changes

### Breaking Changes (Major Version)

| Change | Example | Migration |
|--------|---------|-----------|
| Remove public type | Remove `ISteamGuard` | Update to new interface |
| Change method signature | `GenerateAuthenticationCode(string, long)` | Update call sites |
| Change return type | Return `CodeResult` instead of `string` | Handle new type |
| Remove constructor | Remove `SteamGuard()` default | Use `SteamGuard(ITimeStepProvider)` |
| Change exception type | Throw `SteamGuardException` instead of `ArgumentException` | Update catch blocks |
| Change namespace | Move to `SteamGuard.TOTP.V2` | Update using statements |

### Non-Breaking Changes (Minor/Patch)

| Change | Version Bump |
|--------|--------------|
| Add new method to interface | Minor (if interface) / Patch (if class only) |
| Add new overload | Minor |
| Add new implementation | Minor |
| Fix bug in computation | Patch |
| Improve performance | Patch |
| Update dependencies | Patch (if compatible) |
| Add documentation | Patch |

## Change Process

### 1. Propose Change

1. Open GitHub Issue with `enhancement` or `bug` label
2. Describe the change and rationale
3. Discuss API design if applicable

### 2. Implement Change

```bash
# Create feature branch
git checkout -b feature/description

# Make changes
# - Update source code
# - Add/update tests
# - Update documentation

# Run quality checks
dotnet build -warnaserror
dotnet test
dotnet format --verify-no-changes
```

### 3. Update Version

```bash
# For patch (bug fix)
# Edit SteamGuard.TOTP.csproj: <Version>1.0.1</Version>

# For minor (new feature)
# Edit SteamGuard.TOTP.csproj: <Version>1.1.0</Version>

# For major (breaking)
# Edit SteamGuard.TOTP.csproj: <Version>2.0.0</Version>
```

### 4. Update Documentation

- Update relevant `.md` files in `docs/`
- Update `CHANGELOG.md` (if exists)
- Update `README.md` if user-facing

### 5. Submit PR

1. Push branch
2. Open Pull Request targeting `master`
3. Ensure CI passes
4. Request review

### 6. Release

```bash
# After PR merged
git checkout master
git pull

# Tag release
git tag -a v1.0.1 -m "Release 1.0.1"
git push origin v1.0.1

# GitHub Actions will build, test, and create release
```

## Migration Guides

### From 1.x to 2.0 (Hypothetical)

If a major version introduces breaking changes:

```csharp
// v1.x
var guard = new SteamGuard();
string code = guard.GenerateAuthenticationCode(secret);

// v2.0 - if constructor changed
var provider = new TimeStepProvider();
var guard = new SteamGuard(provider);
string code = guard.GenerateAuthenticationCode(secret);

// v2.0 - if return type changed
CodeResult result = guard.GenerateAuthenticationCode(secret);
string code = result.Code;
bool isValid = result.IsValid;
```

### Migration Checklist

- [ ] Update package reference
- [ ] Fix compiler errors
- [ ] Run tests
- [ ] Verify behavior
- [ ] Update documentation

## Deprecation Policy

### Marking Obsolete

```csharp
[Obsolete("Use GenerateAuthenticationCode(string) instead. Will be removed in 2.0.", false)]
public string GenerateCode(string secret) => GenerateAuthenticationCode(secret);
```

### Deprecation Timeline

| Phase | Version | Duration |
|-------|---------|----------|
| Mark obsolete | Minor | Immediate |
| Warning | Minor + 1 | 1 release |
| Error | Major | Next major |
| Removal | Major | Next major |

## Compatibility Guarantees

### Guaranteed Stable

- `ISteamGuard.GenerateAuthenticationCode(string)`
- `ITimeStepProvider.GetCurrentTimeStep()`
- `SteamGuard` constructors
- `TimeStepProvider` constructor
- Steam alphabet (`23456789BCDFGHJKMNPQRTVWXY`)
- 30-second time step
- 5-character output
- HMAC-SHA1 algorithm

### Not Guaranteed

- Internal methods (`ComputeTotp`, `EncodeToBase32`, `GenerateCodeFromBinCode`)
- Exception messages (exact text)
- Performance characteristics
- Dependency versions (NuciExtensions)

## Testing Changes

### Required Tests for Changes

| Change Type | Required Tests |
|-------------|----------------|
| Algorithm modification | All known vector tests + new vectors |
| New feature | Unit tests covering happy path + edge cases |
| Bug fix | Regression test for the bug |
| Refactoring | All existing tests pass |

### Test Commands

```bash
# Quick test
dotnet test --no-build

# Full test with coverage
dotnet test /p:CollectCoverage=true

# Specific test class
dotnet test --filter "ClassName=SteamGuardTests"
```

## Documentation Updates

### Files to Update

| Change | Files |
|--------|-------|
| New public API | `docs/api-reference/INDEX.md`, `docs/api-reference/isteamguard.md` or `itimestep-provider.md` |
| New feature | `docs/api-usage-examples.md`, `docs/quick-start.md` |
| Breaking change | `docs/change-guide.md`, `docs/ambiguities-and-open-questions.md` |
| Dependency update | `docs/dependencies.md` |
| Architecture change | `docs/architecture.md`, `docs/components/*.md` |

## Rollback Procedure

### If Release Has Critical Issue

```bash
# 1. Revert tag
git tag -d v1.0.1
git push origin :refs/tags/v1.0.1

# 2. Fix issue on master or hotfix branch
git checkout -b hotfix/issue-description master

# 3. Release patch
# Update version to 1.0.2
git tag -a v1.0.2 -m "Release 1.0.2 - Hotfix"
git push origin v1.0.2
```

### NuGet Package Unlisting

```bash
# If package already published
dotnet nuget delete SteamGuard.TOTP 1.0.1 --source https://api.nuget.org/v3/index.json --api-key $NUGET_API_KEY
```

## Related Documents

- [Contributing](../CONTRIBUTING.md)
- [Build and Deployment](build-and-deployment.md)
- [Design Decisions](design-decisions.md)
- [Dependencies](dependencies.md)