# Testing

## Overview

This document describes the testing strategy, test structure, and how to run tests for the SteamGuard.TOTP library.

## Test Project Structure

```
SteamGuard.TOTP.UnitTests/
├── SteamGuard.TOTP.UnitTests.csproj
├── SteamGuardTests.cs
└── Utils/
    └── FixedTimeStepProvider.cs
```

## Test Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.NET.Test.Sdk | 18.3.0 | Test runner |
| NUnit | 4.5.1 | Test framework |
| NUnit3TestAdapter | 6.2.0 | VS Test integration |
| Moq | 4.20.72 | Mocking |

## Running Tests

### All Tests

```bash
# From solution root
dotnet test SteamGuard.TOTP.sln --configuration Release

# From test project
dotnet test SteamGuard.TOTP.UnitTests/SteamGuard.TOTP.UnitTests.csproj --configuration Release
```

### With Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### With TRX Output

```bash
dotnet test --logger "trx;LogFileName=test_results.trx"
```

### Specific Tests

```bash
# By class
dotnet test --filter "FullyQualifiedName~SteamGuardTests"

# By method
dotnet test --filter "FullyQualifiedName~GenerateAuthenticationCode_KnownVector"

# By trait
dotnet test --filter "Category=Unit"
```

### With Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./coverage.xml
```

## Test Categories

### 1. Known Vector Tests (RFC 6238)

```csharp
[Test]
public void GenerateAuthenticationCode_KnownVector_ReturnsExpectedCode()
{
    var provider = new FixedTimeStepProvider(55752480);
    var steamGuard = new SteamGuard(provider);

    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");

    Assert.That(code, Is.EqualTo("R73NH"));
}
```

### 2. Code Format Tests

```csharp
[Test]
public void GenerateAuthenticationCode_ReturnsFiveCharacters()
{
    var steamGuard = new SteamGuard();
    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");

    Assert.That(code.Length, Is.EqualTo(5));
}

[Test]
public void GenerateAuthenticationCode_UsesSteamAlphabet()
{
    var steamGuard = new SteamGuard();
    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");

    Assert.That(code.All(c => "23456789BCDFGHJKMNPQRTVWXY".Contains(c)), Is.True);
}
```

### 3. Error Handling Tests

```csharp
[Test]
public void GenerateAuthenticationCode_NullSecret_ThrowsArgumentNullException()
{
    var steamGuard = new SteamGuard();

    Assert.Throws<ArgumentNullException>(() => steamGuard.GenerateAuthenticationCode(null));
}

[Test]
public void GenerateAuthenticationCode_EmptySecret_ThrowsArgumentException()
{
    var steamGuard = new SteamGuard();

    Assert.Throws<ArgumentException>(() => steamGuard.GenerateAuthenticationCode(""));
}

[Test]
public void GenerateAuthenticationCode_InvalidBase32_ThrowsFormatException()
{
    var steamGuard = new SteamGuard();

    Assert.Throws<FormatException>(() => steamGuard.GenerateAuthenticationCode("INVALID!@#"));
}
```

### 4. Time Step Tests

```csharp
[Test]
public void TimeStepProvider_ReturnsNonNegative()
{
    var provider = new TimeStepProvider();

    Assert.That(provider.GetCurrentTimeStep(), Is.GreaterThanOrEqualTo(0));
}

[Test]
public void FixedTimeStepProvider_ReturnsFixedValue()
{
    var provider = new FixedTimeStepProvider(123456789L);

    Assert.That(provider.GetCurrentTimeStep(), Is.EqualTo(123456789L));
}
```

### 5. Mocking Tests

```csharp
[Test]
public void GenerateAuthenticationCode_UsesInjectedTimeStep()
{
    var mockProvider = new Mock<ITimeStepProvider>();
    mockProvider.Setup(p => p.GetCurrentTimeStep()).Returns(123456789L);

    var steamGuard = new SteamGuard(mockProvider.Object);
    string code = steamGuard.GenerateAuthenticationCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ");

    mockProvider.Verify(p => p.GetCurrentTimeStep(), Times.Once);
}
```

## Test Coverage

### Current Coverage

| Component | Coverage |
|-----------|----------|
| `SteamGuard.GenerateAuthenticationCode` | 100% |
| `SteamGuard` constructors | 100% |
| `TimeStepProvider.GetCurrentTimeStep` | 100% |
| `FixedTimeStepProvider.GetCurrentTimeStep` | 100% |
| Error paths | 100% |

### Coverage Gaps

| Area | Status | Reason |
|------|--------|--------|
| Internal methods | Not covered | Implementation details |
| Clock skew scenarios | Not covered | Requires time manipulation |
| Concurrent access | Not covered | Thread safety by design |

## CI/CD Integration

### GitHub Actions

```yaml
# .github/workflows/dotnet.yml
- name: Test
  run: dotnet test SteamGuard.TOTP.sln --configuration Release --no-build --logger "trx"
```

### Quality Gates

```yaml
# Required for PR merge
- All tests pass
- No test regressions
- Coverage maintained
```

## Test Maintenance

### Adding New Tests

1. Add test method to `SteamGuardTests.cs`
2. Follow naming convention: `MethodName_Scenario_ExpectedResult`
3. Use `FixedTimeStepProvider` for deterministic tests
4. Add known vectors for new algorithms

### Updating Tests

- Update when algorithm changes
- Add regression tests for bugs
- Keep test vectors synchronized with Steam

## Debugging Tests

### Visual Studio

1. Open Test Explorer
2. Right-click test → Debug
3. Set breakpoints in test or source

### VS Code

```json
// launch.json
{
  "type": "coreclr",
  "request": "launch",
  "name": "Debug Test",
  "program": "dotnet",
  "args": ["test", "--filter", "FullyQualifiedName~TestMethodName"],
  "cwd": "${workspaceFolder}/SteamGuard.TOTP.UnitTests"
}
```

### Command Line

```bash
# Run single test with output
dotnet test --filter "FullyQualifiedName~TestMethodName" --logger "console;verbosity=detailed"
```

## Related Documents

- [Test Execution Flow](../flows/test-execution.md)
- [Core Implementation](../components/core-implementation.md)
- [Error Handling](error-handling.md)
- [Build and Deployment](build-and-deployment.md)