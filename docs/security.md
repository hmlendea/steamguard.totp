# Security

## Overview

This document describes the security considerations, threat model, and best practices for using the SteamGuard.TOTP library.

## Threat Model

### Assets to Protect

| Asset | Sensitivity | Protection |
|-------|-------------|------------|
| Shared secret | **Critical** | Never logged, stored, or transmitted |
| Generated codes | **High** | Valid for 30 seconds, not logged |
| Time synchronization | **Medium** | System clock dependency |

### Threat Actors

| Actor | Capability | Mitigation |
|-------|------------|------------|
| Local attacker | Memory access | No secret persistence |
| Network attacker | MITM | No network I/O |
| Malicious dependency | Supply chain | Pinned dependencies, minimal deps |
| Insider | Code access | Open source, auditable |

## Security Properties

### Confidentiality

- **Shared secret**: Never written to logs, files, or network
- **Generated codes**: Never logged by library
- **Memory**: Secrets exist only as method parameters and local variables

### Integrity

- **Algorithm**: Fixed HMAC-SHA1, cannot be changed at runtime
- **Parameters**: Time step (30s), code length (5), alphabet fixed
- **Dependencies**: Pinned versions, no auto-updates

### Availability

- **No external dependencies**: No network calls that can fail
- **No blocking operations**: Sub-millisecond execution
- **No resource exhaustion**: Minimal allocations

## Secure Usage

### Secret Management

```csharp
// ✅ Good: Environment variable
string secret = Environment.GetEnvironmentVariable("STEAM_SHARED_SECRET");

// ✅ Good: Azure Key Vault
var client = new SecretClient(vaultUri, credential);
KeyVaultSecret secret = await client.GetSecretAsync("steam-shared-secret");

// ✅ Good: AWS Secrets Manager
var request = new GetSecretValueRequest { SecretId = "steam-shared-secret" };
var response = await client.GetSecretValueAsync(request);

// ❌ Bad: Hardcoded
string secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

// ❌ Bad: Config file (unless encrypted)
string secret = configuration["Steam:Secret"];

// ❌ Bad: Command line argument
string secret = args[0];
```

### Input Validation

```csharp
// Validate before calling library
if (string.IsNullOrWhiteSpace(secret))
    throw new ArgumentException("Secret required");

if (!SteamGuardValidator.IsValidSharedSecret(secret))
    throw new ArgumentException("Invalid secret format");

string code = steamGuard.GenerateAuthenticationCode(secret);
```

### Output Handling

```csharp
// Use code immediately, don't store
string code = steamGuard.GenerateAuthenticationCode(secret);
await steamClient.ConfirmTradeAsync(tradeId, code);
// Code goes out of scope, eligible for GC
```

## Cryptographic Security

### Algorithm

| Property | Value | Security Note |
|----------|-------|---------------|
| MAC | HMAC-SHA1 | Matches Steam; SHA-1 collision resistance not required for HMAC |
| Key derivation | None (direct use) | Secret used directly as HMAC key |
| Time step | 30 seconds | RFC 6238 standard |
| Truncation | RFC 4226 dynamic | Standard TOTP truncation |
| Output encoding | Custom base-26 | Steam-specific alphabet |

### Known Limitations

| Limitation | Impact | Mitigation |
|------------|--------|------------|
| HMAC-SHA1 | Theoretical collision attacks | Not applicable to HMAC; Steam uses SHA-1 |
| No clock skew handling | Codes may fail if clock off | Deploy NTP |
| 5-char codes | ~26^5 = 11.8M combinations | Sufficient for 30s window |
| No rate limiting | Unlimited generation | Application-level responsibility |

## Dependency Security

### Direct Dependencies

| Package | Version | Security Review |
|---------|---------|-----------------|
| NuciExtensions | 5.3.1 | Compile-time only; no runtime network calls |

### Transitive Dependencies

All transitive dependencies are .NET BCL (Microsoft-maintained):
- `System.Security.Cryptography`
- `System.Net`
- `System.Linq`
- `System.Runtime`

### Vulnerability Scanning

```bash
# Check for known vulnerabilities
dotnet list package --vulnerable

# Check for outdated packages
dotnet list package --outdated

# Enable security analyzers
dotnet build /p:EnableNETAnalyzers=true
```

## Secure Deployment

### Container Security

```dockerfile
# Use distroless or minimal base
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine

# Run as non-root
USER 1000

# Read-only filesystem
# (requires volume for any writes)
```

### Kubernetes Security

```yaml
securityContext:
  runAsNonRoot: true
  runAsUser: 1000
  readOnlyRootFilesystem: true
  allowPrivilegeEscalation: false
  capabilities:
    drop:
    - ALL
```

## Compliance

### Standards Alignment

| Standard | Alignment |
|----------|-----------|
| RFC 6238 (TOTP) | Full compliance |
| RFC 4226 (HOTP) | Truncation compliance |
| RFC 4648 (Base32) | Decoding compliance |
| FIPS 198-1 (HMAC) | Uses .NET HMACSHA1 |

### Data Protection

| Regulation | Consideration |
|------------|---------------|
| GDPR | No personal data processed |
| CCPA | No personal data processed |
| SOC 2 | No data storage/transmission |
| PCI DSS | No cardholder data |

## Incident Response

### If Secret Compromised

1. **Immediate**: Revoke Steam mobile authenticator
2. **Rotate**: Generate new shared secret in Steam
3. **Update**: Deploy new secret to all applications
4. **Audit**: Check for unauthorized trade confirmations

### If Code Intercepted

- Codes valid for 30 seconds only
- Attacker needs secret to generate future codes
- Rotate secret if interception suspected

## Security Testing

### Static Analysis

```bash
# Run security analyzers
dotnet build /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest

# SonarQube / CodeQL
# Configure in CI pipeline
```

### Penetration Testing

Focus areas:
1. Secret handling in consumer applications
2. Clock synchronization
3. Rate limiting on code generation endpoints
4. Logging configuration (ensure no secrets logged)

## Security Checklist

### Development

- [ ] Never log shared secrets
- [ ] Never log generated codes
- [ ] Validate input before calling library
- [ ] Use secure secret storage
- [ ] Pin dependency versions

### Deployment

- [ ] Enable NTP on all hosts
- [ ] Use read-only filesystems
- [ ] Run as non-root
- [ ] Scan for vulnerabilities
- [ ] Monitor for anomalous code generation

### Operations

- [ ] Rotate secrets periodically
- [ ] Monitor failed code validations
- [ ] Alert on clock skew
- [ ] Review access to secret storage

## Related Documents

- [Error Handling](error-handling.md)
- [Logging](logging.md)
- [Configuration](configuration.md)
- [Dependencies](dependencies.md)
- [PRIVACY.md](../PRIVACY.md)
- [SECURITY.md](../SECURITY.md)