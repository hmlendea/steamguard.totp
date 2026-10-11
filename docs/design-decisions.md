# Design Decisions

This document records key architectural and design decisions for the SteamGuard.TOTP library with their rationale.

## Decision Log

### 1. Minimal Interface-Based API

**Decision:** Expose only `ISteamGuard` and `ITimeStepProvider` as public types. `SteamGuard` and `TimeStepProvider` are public but concrete implementations.

**Rationale:**
- Reduces API surface area
- Enables dependency injection and testing
- Allows future implementation changes without breaking consumers
- Follows .NET library best practices (program to interfaces)

**Alternatives considered:**
- Static class with static methods — rejected (not testable, not DI-friendly)
- Multiple interfaces — rejected (YAGNI, single responsibility)

### 2. HMAC-SHA1 Algorithm (Fixed)

**Decision:** Hard-code HMAC-SHA1 as the only supported algorithm.

**Rationale:**
- Steam Guard specification mandates HMAC-SHA1
- RFC 6238 allows SHA-256/512 but Steam doesn't use them
- Adding algorithm parameter would complicate API for no benefit

**Alternatives considered:**
- Configurable algorithm — rejected (Steam compatibility requirement)
- Support multiple algorithms — rejected (scope creep)

### 3. 30-Second Time Step (Fixed)

**Decision:** Hard-code 30-second interval in `TimeStepProvider.Interval`.

**Rationale:**
- TOTP RFC 6238 default and Steam implementation
- Changing interval breaks compatibility with Steam
- No known use case for different interval

**Alternatives considered:**
- Configurable interval — rejected (Steam compatibility)

### 4. 5-Character Output (Fixed)

**Decision:** Hard-code 5-character code length.

**Rationale:**
- Steam Guard mobile authenticator produces 5 characters
- RFC 4226 allows 6-8 digits but Steam uses 5 alphanumeric
- Fixed length simplifies implementation and testing

### 5. Steam Character Alphabet (Fixed)

**Decision:** Use fixed alphabet `23456789BCDFGHJKMNPQRTVWXY`.

**Rationale:**
- Steam-specific alphabet excludes ambiguous characters
- Matches Steam mobile authenticator exactly
- 32 characters = base-32 encoding of truncated HMAC

### 6. No Input Validation

**Decision:** Library does not validate `totpKey` format; caller responsibility.

**Rationale:**
- Minimal API surface
- Performance: avoid double validation (caller + library)
- Fail-fast: invalid input produces clear exceptions anyway
- Consistent with low-level crypto library patterns

**Trade-off:** Less friendly error messages for invalid input.

### 7. Base32 Decoding Without Padding

**Decision:** Implement RFC 4648 Base32 decoding without padding support.

**Rationale:**
- Steam shared secrets don't use padding
- Simpler implementation
- Padding rarely used in TOTP secrets

**Limitation:** Cannot decode padded Base32 strings.

### 8. NuciExtensions for Time Calculation

**Decision:** Depend on `NuciExtensions.DateTimeExtensions.GetElapsedUnixTime` instead of manual calculation.

**Rationale:**
- Avoids `DateTime` vs `DateTimeOffset` pitfalls
- Handles leap seconds, timezone edge cases
- Well-tested library dependency
- Single responsibility: time calculation delegated

**Dependency:** `NuciExtensions` 5.3.1 (compile-time only)

### 9. Single Target Framework (net10.0)

**Decision:** Target only `net10.0`, no multi-targeting.

**Rationale:**
- Simplifies build, packaging, testing
- Modern .NET with long-term support
- No legacy framework constraints
- Reduces CI matrix complexity

**Trade-off:** Consumers on older .NET versions cannot use library.

### 10. GPL-3.0-or-later License

**Decision:** Use GPL-3.0-or-later (copyleft).

**Rationale:**
- Ensures derivative works remain free
- Aligns with author's values
- Compatible with GPL ecosystem

**Impact:** Consumers must comply with GPL (source disclosure, same license).

### 11. Constructor Injection for Time Provider

**Decision:** `SteamGuard` accepts `ITimeStepProvider` via constructor (with default).

**Rationale:**
- Enables deterministic testing with `FixedTimeStepProvider`
- Follows DI best practices
- Default constructor maintains backward compatibility
- No service locator or ambient context

### 12. Stateless Design

**Decision:** All components stateless; no instance fields except injected dependencies.

**Rationale:**
- Inherent thread safety
- No synchronization needed
- Simple reasoning about behavior
- Suitable for singleton DI registration

### 13. HMACSHA1 Disposal

**Decision:** Wrap `HMACSHA1` in `using` statement per call.

**Rationale:**
- `HMACSHA1` implements `IDisposable`
- Proper resource cleanup
- No pooling needed (lightweight allocation)

### 14. Network Byte Order for Time Step

**Decision:** Use `IPAddress.HostToNetworkOrder` for time step serialization.

**Rationale:**
- RFC 4226 / RFC 6238 require big-endian encoding
- Ensures cross-platform consistency
- `BitConverter` is little-endian on x86/x64

### 15. Dynamic Truncation per RFC 4226

**Decision:** Implement standard HOTP dynamic truncation (offset from last 4 bits).

**Rationale:**
- RFC 4226 standard for HOTP/TOTP
- Proven cryptographic construction
- Matches Steam implementation

### 16. Test Vectors from Known Sources

**Decision:** Use validated test vectors from Steam-compatible implementations.

**Rationale:**
- Ensures algorithmic correctness
- Regression detection
- Documents expected behavior

### 17. NUnit + Moq for Testing

**Decision:** Use NUnit 4.x with Moq for mocking.

**Rationale:**
- Modern, actively maintained
- Good .NET integration
- Expressive assertion syntax
- Moq for interface mocking

### 18. No Logging or Telemetry

**Decision:** Zero logging, metrics, or telemetry in library.

**Rationale:**
- Privacy: no data collection
- Performance: zero overhead
- Simplicity: no configuration needed
- Caller can add logging if desired

### 19. No Async API

**Decision:** Synchronous `GenerateAuthenticationCode` only.

**Rationale:**
- Computation is fast (< 1ms)
- No I/O to await
- Simpler API
- Caller can wrap in `Task.Run` if needed

### 20. No Serialization Support

**Decision:** Interfaces and classes not marked `[Serializable]` or with JSON attributes.

**Rationale:**
- Not a DTO library
- Interfaces shouldn't be serialized
- Consumers can wrap if needed

## Decision Tracking

| ID | Decision | Date | Status |
|----|----------|------|--------|
| 001 | Minimal interface-based API | 2026 | Active |
| 002 | HMAC-SHA1 fixed | 2026 | Active |
| 003 | 30-second interval fixed | 2026 | Active |
| 004 | 5-character output fixed | 2026 | Active |
| 005 | Steam alphabet fixed | 2026 | Active |
| 006 | No input validation | 2026 | Active |
| 007 | Base32 without padding | 2026 | Active |
| 008 | NuciExtensions for time | 2026 | Active |
| 009 | Single target net10.0 | 2026 | Active |
| 010 | GPL-3.0-or-later | 2026 | Active |
| 011 | Constructor injection | 2026 | Active |
| 012 | Stateless design | 2026 | Active |
| 013 | HMACSHA1 disposal | 2026 | Active |
| 014 | Network byte order | 2026 | Active |
| 015 | RFC 4226 truncation | 2026 | Active |
| 016 | Validated test vectors | 2026 | Active |
| 017 | NUnit + Moq | 2026 | Active |
| 018 | No logging/telemetry | 2026 | Active |
| 019 | No async API | 2026 | Active |
| 020 | No serialization | 2026 | Active |

## Revisiting Decisions

Decisions should be revisited when:
- Steam changes its authenticator algorithm
- .NET introduces breaking changes affecting the library
- Security vulnerability discovered in HMAC-SHA1 for this use case
- Consumer demand for multi-targeting emerges
- License compatibility issues arise

To propose a decision change, open an issue with the decision ID and rationale.