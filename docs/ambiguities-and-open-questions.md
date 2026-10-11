# Ambiguities and Open Questions

## Overview

This document tracks known ambiguities, open questions, and areas requiring clarification in the SteamGuard.TOTP library.

## Open Questions

### 1. Steam Alphabet Completeness

**Question**: Is the 26-character Steam alphabet (`23456789BCDFGHJKMNPQRTVWXY`) complete and final?

**Context**: The alphabet excludes `0, 1, 8, I, L, O, S, U, Z` to avoid visual confusion. Steam's official documentation doesn't explicitly publish this alphabet.

**Impact**: If Steam changes the alphabet, all generated codes would be invalid.

**Status**: Unconfirmed; based on community reverse-engineering.

### 2. Base32 Encoding Variant

**Question**: Which Base32 variant does Steam use for shared secrets?

**Context**: The library uses RFC 4648 Base32 (no padding, case-insensitive). Steam's QR codes and manual entry may use a different variant.

**Impact**: Secrets from different sources may not decode correctly.

**Status**: Assumed RFC 4648; needs verification with official Steam sources.

### 3. Time Synchronization Requirements

**Question**: What clock skew tolerance does Steam accept?

**Context**: RFC 6238 recommends accepting codes from adjacent time steps (±1). Steam's actual tolerance is undocumented.

**Impact**: Clock drift could cause valid codes to be rejected.

**Status**: Unknown; library generates current time step only.

### 4. Secret Length Requirements

**Question**: What are the minimum/maximum shared secret lengths?

**Context**: The library accepts any valid Base32 string. Steam's actual constraints are undocumented.

**Impact**: Very short or long secrets may behave unexpectedly.

**Status**: Unconstrained in library; Steam may enforce limits.

### 5. HMAC-SHA1 Deprecation Timeline

**Question**: When will Steam migrate from HMAC-SHA1 to HMAC-SHA256/512?

**Context**: RFC 6238 allows SHA-256/512. Industry is moving away from SHA-1.

**Impact**: Library would need breaking changes to support new algorithms.

**Status**: No official announcement from Steam.

## Ambiguities in Current Implementation

### 1. Error Message Specificity

**Ambiguity**: `ArgumentException` for invalid Base32 doesn't distinguish between:
- Invalid characters
- Incorrect padding
- Empty string after filtering

**Proposed Resolution**: Custom exception types or richer error codes.

### 2. Case Sensitivity of Shared Secret

**Ambiguity**: Library accepts both uppercase and lowercase. Is this correct?

**Context**: Base32 is case-insensitive per RFC 4648. Steam's UI shows uppercase.

**Status**: Current behavior is correct per RFC.

### 3. Whitespace Handling

**Ambiguity**: Library doesn't trim whitespace from shared secret.

**Context**: User may copy-paste with trailing spaces/newlines.

**Proposed Resolution**: Add `Trim()` before validation.

### 4. Null vs Empty String Handling

**Ambiguity**: `null` throws `ArgumentNullException`, empty string throws `ArgumentException`.

**Context**: Consistent with .NET conventions but could be unified.

**Status**: Current behavior follows .NET guidelines.

## Future Considerations

### 1. Multi-Account Support

**Question**: Should the library support multiple shared secrets simultaneously?

**Current**: Single secret per `SteamGuard` instance.

**Proposed**: Add `GenerateAuthenticationCode(string secret, long timeStep)` overload.

### 2. Batch Code Generation

**Question**: Should the library support generating codes for multiple time steps?

**Use Case**: Displaying current + next code for user convenience.

**Proposed**: Add `GenerateAuthenticationCodes(string secret, int count)` method.

### 3. QR Code Generation

**Question**: Should the library generate Steam-compatible QR codes?

**Context**: Steam uses `otpauth://totp/Steam:account?secret=...&issuer=Steam` format.

**Proposed**: Add `GenerateQrCodeUri(string accountName, string secret)` method.

### 4. Async API

**Question**: Should async overloads be added?

**Context**: Current API is synchronous and fast. Async would add complexity without benefit.

**Status**: Not needed; computation is sub-millisecond.

## Decision Tracking

| ID | Question | Status | Decision | Date |
|----|----------|--------|----------|------|
| AQ-001 | Steam alphabet completeness | Open | Monitor Steam updates | - |
| AQ-002 | Base32 variant | Assumed | RFC 4648 | - |
| AQ-003 | Clock skew tolerance | Unknown | Generate current only | - |
| AQ-004 | Secret length limits | Unconstrained | Accept any valid Base32 | - |
| AQ-005 | SHA-1 deprecation | Unknown | Monitor Steam announcements | - |

## Resolution Process

1. **Monitor** Steam community forums and official channels
2. **Test** against live Steam Guard endpoints when possible
3. **Document** findings in this file
4. **Propose** changes via GitHub Issues
5. **Implement** with version bump if breaking

## Contributing Clarifications

If you have information about any open question:
1. Open a GitHub Issue with `clarification` label
2. Provide evidence (Steam documentation, network captures, official statements)
3. Reference the question ID (e.g., AQ-001)

## Related Documents

- [Design Decisions](design-decisions.md)
- [API Reference](api-reference/INDEX.md)
- [Code Generation Flow](../flows/code-generation.md)
- [Security](security.md)