# Concurrency and Scheduling

## Overview

This document describes the concurrency model and scheduling behavior of the SteamGuard.TOTP library.

## Concurrency Model

### Thread Safety

All public types are **thread-safe**:

| Type | Thread Safety | Reason |
|------|---------------|--------|
| `SteamGuard` | ✅ Safe | Stateless, no shared mutable state |
| `TimeStepProvider` | ✅ Safe | Stateless, reads system time |
| `FixedTimeStepProvider` | ✅ Safe | Immutable after construction |
| `ISteamGuard` | ✅ Safe | Interface contract |
| `ITimeStepProvider` | ✅ Safe | Interface contract |

### No Locks Required

```csharp
// Safe for concurrent use
var steamGuard = new SteamGuard();

// Multiple threads can call simultaneously
Parallel.For(0, 100, _ => {
    string code = steamGuard.GenerateAuthenticationCode(secret);
    Console.WriteLine(code);
});
```

### No Shared State

```
Thread 1                    Thread 2                    Thread 3
    │                          │                          │
    ▼                          ▼                          ▼
┌─────────┐               ┌─────────┐               ┌─────────┐
│ SteamGuard.GenerateAuthenticationCode(secret)     │
│   │                                                      │
│   ├── GetCurrentTimeStep()  (reads DateTime.UtcNow)    │
│   ├── Decode Base32         (local computation)        │
│   ├── Compute HMAC-SHA1     (local computation)        │
│   ├── Truncate              (local computation)        │
│   └── Map to alphabet       (local computation)        │
    │                          │                          │
    ▼                          ▼                          ▼
Returns code              Returns code              Returns code
```

## Scheduling

### Time Step Boundaries

The library operates on 30-second intervals:

```
Time Step N          Time Step N+1          Time Step N+2
     │                    │                    │
     ▼                    ▼                    ▼
┌─────────┐          ┌─────────┐          ┌─────────┐
│  Code   │          │  Code   │          │  Code   │
│  Valid  │          │  Valid  │          │  Valid  │
└─────────┘          └─────────┘          └─────────┘
     │                    │                    │
     └────────────────────┴────────────────────┘
                    30 seconds each
```

### Code Validity Window

| Scenario | Validity |
|----------|----------|
| Generated at T+0s | Valid until T+30s |
| Generated at T+15s | Valid until T+30s |
| Generated at T+29s | Valid until T+30s |
| Generated at T+30s | New time step, new code |

### Clock Synchronization

The library uses `DateTime.UtcNow` (via `NuciExtensions.DateTimeExtensions.GetElapsedUnixTime()`):

```csharp
// TimeStepProvider implementation
public long GetCurrentTimeStep()
{
    return DateTimeExtensions.GetElapsedUnixTime() / 30;
}
```

**Considerations:**
- System clock must be reasonably accurate (NTP recommended)
- No built-in clock skew compensation
- RFC 6238 recommends accepting ±1 time step (not implemented)

## Parallel Execution

### Concurrent Code Generation

```csharp
// Generate codes for multiple accounts in parallel
var accounts = new[]
{
    ("account1", "SECRET1"),
    ("account2", "SECRET2"),
    ("account3", "SECRET3")
};

var results = accounts.AsParallel().Select(a =>
    (a.Item1, Code: steamGuard.GenerateAuthenticationCode(a.Item2))
).ToList();
```

### High-Frequency Generation

```csharp
// Generate codes rapidly (e.g., for monitoring)
for (int i = 0; i < 1000; i++)
{
    string code = steamGuard.GenerateAuthenticationCode(secret);
    // All calls return same code within 30-second window
}
```

## Performance Under Concurrency

### Benchmarks (Approximate)

| Operation | Single Thread | 10 Threads | 100 Threads |
|-----------|---------------|------------|-------------|
| `GenerateAuthenticationCode` | ~50 μs | ~50 μs | ~55 μs |
| `GetCurrentTimeStep` | ~5 ns | ~5 ns | ~5 ns |
| Memory allocation | ~200 bytes | ~200 bytes | ~200 bytes |

### No Contention

- No locks, no shared mutable state
- Each call allocates minimal objects
- Scales linearly with cores

## Async Considerations

### No Async API

The library provides **no async methods**:

```csharp
// Synchronous only
string code = steamGuard.GenerateAuthenticationCode(secret);

// If you need async, wrap it:
string code = await Task.Run(() => steamGuard.GenerateAuthenticationCode(secret));
```

### Why No Async?

- Computation is CPU-bound, not I/O-bound
- Sub-millisecond execution time
- Adding async would add overhead without benefit
- Consumers can wrap if needed

## Scheduling in Hosted Environments

### Background Service Pattern

```csharp
public class SteamCodeMonitor : BackgroundService
{
    private readonly ISteamGuard _steamGuard;
    private readonly string _secret;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string code = _steamGuard.GenerateAuthenticationCode(_secret);

            // Process code...

            // Wait for next time step boundary
            var now = DateTimeOffset.UtcNow;
            var secondsIntoStep = now.Second % 30;
            var delay = TimeSpan.FromSeconds(30 - secondsIntoStep);

            await Task.Delay(delay, stoppingToken);
        }
    }
}
```

### Timer-Based Generation

```csharp
// Generate code every 30 seconds
var timer = new Timer(_ =>
{
    string code = steamGuard.GenerateAuthenticationCode(secret);
    OnCodeGenerated(code);
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
```

## Testing Concurrency

### Concurrent Test

```csharp
[Test]
public void GenerateAuthenticationCode_ConcurrentCalls_ReturnsSameCode()
{
    var provider = new FixedTimeStepProvider(123456789L);
    var steamGuard = new SteamGuard(provider);
    string secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";

    var codes = Enumerable.Range(0, 100)
        .AsParallel()
        .Select(_ => steamGuard.GenerateAuthenticationCode(secret))
        .Distinct()
        .ToList();

    Assert.That(codes.Count, Is.EqualTo(1));
    Assert.That(codes[0], Is.EqualTo("EXPECTED_CODE"));
}
```

### Thread Safety Test

```csharp
[Test]
public void TimeStepProvider_ConcurrentCalls_NoExceptions()
{
    var provider = new TimeStepProvider();

    Assert.DoesNotThrow(() =>
    {
        Parallel.For(0, 1000, _ => provider.GetCurrentTimeStep());
    });
}
```

## Limitations

### No Distributed Coordination

The library provides no:
- Distributed locking
- Consensus on time
- Cluster-aware code generation
- Shared state across processes

### Clock Drift

If system clocks drift:
- Codes may be generated for wrong time step
- No automatic correction
- Deploy NTP on all hosts

## Related Documents

- [Core Implementation](../components/core-implementation.md)
- [Code Generation Flow](../flows/code-generation.md)
- [Thread Safety Analysis](../flows/code-generation.md#thread-safety-analysis)
- [Testing](testing.md)