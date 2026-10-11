# SteamGuard.TOTP Documentation

## Repository Summary

SteamGuard.TOTP is a lightweight .NET library for generating Steam Guard authentication codes from a Base32-encoded TOTP shared secret. It exposes a minimal public API (`ISteamGuard`) with a single method `GenerateAuthenticationCode(string totpKey)` and supports dependency injection for deterministic testing through the `ITimeStepProvider` abstraction. The library has no network access, telemetry, or persistent storage.

## Root Document Map

- [ARCHITECTURE.md](../ARCHITECTURE.md) — High-level architecture, components, runtime flow, and design constraints
- [SECURITY.md](../SECURITY.md) — Security policy, supported versions, vulnerability reporting
- [PRIVACY.md](../PRIVACY.md) — Data handling description (no personal data collected)
- [CONTRIBUTING.md](../CONTRIBUTING.md) — Contribution guidelines
- [LICENSE](../LICENSE) — GNU GPL v3.0 or later
- [README.md](../README.md) — Project overview, installation, quick start, API reference

## Documentation Catalogue

### Architecture & Design
- [Architecture](./architecture.md) — Detailed architecture complementing root ARCHITECTURE.md
- [Design Decisions](./design-decisions.md) — Key architectural/design choices and rationale
- [Dependencies](./dependencies.md) — External and internal dependencies
- [Repository Structure](./repository-structure.md) — Source tree layout, module organisation

### Components
- [Components Overview](./components/index.md) — Component catalogue and responsibilities
- [Core Implementation](./components/core-implementation.md) — SteamGuard, TimeStepProvider deep dive
- [Public Contracts](./components/public-contracts.md) — ISteamGuard, ITimeStepProvider interfaces

### Flows
- [Flows Overview](./flows/index.md) — End-to-end execution flows
- [Code Generation Flow](./flows/code-generation.md) — GenerateAuthenticationCode sequence

### API Reference
- [API Reference Index](./api-reference/INDEX.md) — Endpoint summary table (N/A for library)
- [ISteamGuard Reference](./api-reference/isteamguard.md) — Public interface documentation

### Data & State
- [Data Model](./data-model.md) — Domain entities, relationships (minimal for this library)
- [State and Persistence](./state-and-persistence.md) — State management (stateless library)

### Cross-Cutting Concerns
- [Error Handling](./error-handling.md) — Error taxonomy, handling patterns
- [Security](./security.md) — Security model, threats, mitigations (complements root SECURITY.md)
- [Testing](./testing.md) — Test strategy, organisation, coverage
- [Concurrency and Scheduling](./concurrency-and-scheduling.md) — Threading, async, thread-safety

### Operations
- [Build and Deployment](./build-and-deployment.md) — Build pipeline, NuGet packaging, CI/CD
- [Configuration](./configuration.md) — Configuration schema (minimal for this library)
- [Change Guide](./change-guide.md) — How to modify common areas safely

### Reference
- [FAQ](./faq.md) — Frequently asked questions
- [Troubleshooting](./troubleshooting.md) — Common issues and solutions
- [Invariants](./invariants.md) — System-wide invariants and contracts
- [Ambiguities and Open Questions](./ambiguities-and-open-questions.md) — Unresolved items, known gaps
- [Documentation Maintenance](./documentation-maintenance.md) — How to keep docs current, ownership
- [Quick Start](./quick-start.md) — Getting started guide for newcomers
- [Repository Overview](./repository-overview.md) — Repository purpose, scope, entry points

## Navigation Aids

- **Start here (newcomers):** [Quick Start](./quick-start.md) → [Repository Overview](./repository-overview.md) → [Architecture](./architecture.md)
- **Deep dive (component owners):** [Components Overview](./components/index.md) → [Core Implementation](./components/core-implementation.md) → [Public Contracts](./components/public-contracts.md)
- **Flows (debuggers):** [Code Generation Flow](./flows/code-generation.md) → [Testing](./testing.md)

## Maintenance Metadata

- **Last reviewed:** 2026-10-11
- **Owner:** Horațiu Mlendea
- **Coverage status:** Complete for current library scope