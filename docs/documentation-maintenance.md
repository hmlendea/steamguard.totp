# Documentation Maintenance

## Overview

This document describes the processes and standards for maintaining documentation in the SteamGuard.TOTP repository.

## Documentation Structure

```
docs/
├── INDEX.md                          # Master index
├── repository-overview.md            # Purpose, scope, entry points
├── repository-structure.md           # Source tree, modules, configs
├── architecture.md                   # Detailed architecture
├── components/
│   ├── index.md                      # Component catalogue
│   ├── core-implementation.md        # SteamGuard, TimeStepProvider deep dive
│   └── public-contracts.md           # Interfaces, contracts
├── flows/
│   ├── index.md                      # Flow catalogue
│   ├── code-generation.md            # Code generation flow
│   └── test-execution.md             # Test execution flow
├── api-reference/
│   ├── INDEX.md                      # API reference index
│   ├── isteamguard.md                # ISteamGuard reference
│   └── itimestep-provider.md         # ITimeStepProvider reference
├── api-usage-examples.md             # Usage examples
├── ambiguities-and-open-questions.md # Open questions
├── build-and-deployment.md           # Build, pack, publish
├── change-guide.md                   # Versioning, migrations
├── configuration.md                  # Configuration options
├── concurrency-and-scheduling.md     # Threading, scheduling
├── data-model.md                     # Data structures, flow
├── documentation-maintenance.md      # This file
├── error-handling.md                 # Exceptions, patterns
├── faq.md                            # Frequently asked questions
├── invariants.md                     # Guaranteed properties
├── integrations.md                   # Platform integrations
├── logging.md                        # Logging guidance
├── quick-start.md                    # 5-minute start
├── security.md                       # Security considerations
├── state-and-persistence.md          # State management
├── testing.md                        # Testing strategy
└── troubleshooting.md                # Common issues
```

## Maintenance Principles

### 1. Documentation as Code

- Documentation lives in the repository
- Versioned with code
- Reviewed in PRs
- Built and validated in CI

### 2. Single Source of Truth

| Information | Source |
|-------------|--------|
| API signatures | Source code (XML docs) |
| Version | `SteamGuard.TOTP.csproj` |
| Dependencies | `csproj` files |
| Test vectors | `SteamGuardTests.cs` |
| Architecture | `docs/architecture.md` |

### 3. Consistency Standards

| Standard | Rule |
|----------|------|
| Headings | ATX style (`#`, `##`, `###`) |
| Code blocks | Fenced with language |
| Tables | GitHub-flavored markdown |
| Links | Relative, repository-rooted |
| Diagrams | Mermaid.js |

## Update Triggers

### When to Update Documentation

| Trigger | Files to Update |
|---------|-----------------|
| New public API | `api-reference/`, `api-usage-examples.md` |
| Algorithm change | `architecture.md`, `flows/code-generation.md`, `invariants.md` |
| New dependency | `dependencies.md`, `build-and-deployment.md` |
| Breaking change | `change-guide.md`, `README.md` |
| New feature | `api-usage-examples.md`, `quick-start.md` |
| Bug fix | `troubleshooting.md`, `testing.md` |
| Security issue | `security.md`, `SECURITY.md` |

### Automated Checks

```bash
# Validate links
markdown-link-check docs/**/*.md

# Check spelling
cspell docs/**/*.md

# Validate Mermaid diagrams
# (manual review in PR)
```

## Review Process

### PR Requirements

- [ ] Documentation updated for user-facing changes
- [ ] Links verified (relative paths work)
- [ ] Code examples compile
- [ ] Mermaid diagrams render
- [ ] No broken internal references

### Review Checklist

| Item | Verification |
|------|--------------|
| Accuracy | Matches current implementation |
| Completeness | Covers all public API |
| Clarity | Understandable by new contributors |
| Consistency | Follows style guide |
| Links | All internal links resolve |

## Style Guide

### Headings

```markdown
# Document Title (H1 - once per file)

## Section (H2)

### Subsection (H3)

#### Detail (H4)
```

### Code Blocks

````markdown
```csharp
// Always specify language
var steamGuard = new SteamGuard();
string code = steamGuard.GenerateAuthenticationCode(secret);
```
````

### Tables

```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Value 1  | Value 2  | Value 3  |
```

### Links

```markdown
<!-- Relative to repository root -->
[Architecture](docs/architecture.md)

<!-- External -->
[RFC 6238](https://tools.ietf.org/html/rfc6238)
```

### Admonitions

```markdown
> **Note**: Informational note

> **Warning**: Important caution

> **Tip**: Helpful suggestion
```

## Validation

### Pre-commit Hooks

```yaml
# .pre-commit-config.yaml
repos:
  - repo: https://github.com/igorshubovych/markdownlint-cli
    hooks:
      - id: markdownlint
        args: ["--config", ".markdownlint.json"]
```

### CI Validation

```yaml
# .github/workflows/docs.yml
- name: Validate Documentation
  run: |
    markdown-link-check docs/**/*.md
    cspell docs/**/*.md
```

## Versioning

### Documentation Versioning

- Documentation versioned with code (same git history)
- No separate versioning scheme
- Breaking changes documented in `change-guide.md`

### Deprecation Notices

```markdown
> **Deprecated** (since v2.0): Use `NewMethod` instead.
> This method will be removed in v3.0.
```

## Tools

### Recommended Tools

| Tool | Purpose |
|------|---------|
| VS Code + Markdown All in One | Editing |
| markdown-link-check | Link validation |
| cspell | Spell checking |
| Mermaid Live Editor | Diagram preview |
| markdownlint | Linting |

### Local Validation

```bash
# Install tools
npm install -g markdown-link-check cspell markdownlint-cli

# Run checks
markdown-link-check docs/**/*.md
cspell docs/**/*.md
markdownlint docs/**/*.md
```

## Contributing to Documentation

### Adding New Documents

1. Create file in appropriate directory
2. Follow structure of similar documents
3. Add to `docs/INDEX.md` catalogue
4. Update relevant cross-references

### Editing Existing Documents

1. Make minimal, focused changes
2. Update "Last Updated" if present
3. Verify links still work
4. Run validation tools

## Archive Policy

### Obsolete Documents

- Move to `docs/archive/` with date prefix
- Update links in `INDEX.md`
- Keep for historical reference

### Example

```
docs/archive/
├── 2024-01-15-old-architecture.md
└── 2024-03-20-deprecated-api.md
```

## Metrics

### Documentation Health

| Metric | Target |
|--------|--------|
| Link validity | 100% |
| Spell check | 0 errors |
| Coverage (public API) | 100% |
| Freshness | < 30 days since code change |

## Related Documents

- [Contributing](../CONTRIBUTING.md)
- [Change Guide](change-guide.md)
- [Architecture](architecture.md)
- [API Reference](api-reference/INDEX.md)