# Contributing to SteamGuard.TOTP

This document covers the guidelines and processes for contributing to this project, including how to report issues, suggest enhancements, submit code changes, and follow project standards.

## 📑 Table of Contents

- [How to Contribute](#how-to-contribute)
- [Code Style](#code-style)
- [Testing](#testing)
- [Documentation](#documentation)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [License](#license)
- [Getting Help](#getting-help)

## 🤝 How to Contribute

### Reporting Issues

- Search existing issues first.
- Use the issue templates if available.
- Provide clear reproduction steps.
- Include environment details.

### Suggesting Enhancements

- Check the roadmap and existing discussions.
- Explain the use case and expected behavior.
- Consider implementation complexity.

### Code Contributions

#### Prerequisites

- .NET SDK 10.0 or later

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/steamguard.totp.git
cd steamguard.totp

# Install dependencies
dotnet restore
```

#### Making Changes

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes.
4. Run tests: `dotnet test`
5. Run build: `dotnet build`
6. Commit with clear and descriptive messages.
7. Push to your fork.
8. Open a Pull Request.

#### Pull Request Guidelines

- Target the `master` branch.
- Keep PRs focused and atomic.
- Update documentation if applicable.
- Add tests for any new functionality.
- Ensure the CI checks pass.

### Code Style

Follow the project's coding standards:
- C# coding conventions as defined in the repository's `.editorconfig` (if present) and common .NET practices
- Run `dotnet format` before committing (if available)

### Testing

```bash
# Run all tests
dotnet test

# Run tests with verbosity
dotnet test --verbosity normal
```

### Documentation

- Update relevant docs for changes.
- Follow the documentation style guide.
- Preview changes locally if possible.

## 📋 Code of Conduct

This project follows the [Code of Conduct](https://github.com/hmlendea/steamguard.totp/blob/main/CODE_OF_CONDUCT.md) (if available).

## 🔒 Security

Report security vulnerabilities per the [Security Policy](./SECURITY.md).

## 📄 License

By contributing, you agree that your contributions will be licensed under the GNU General Public License v3.0 or later.

## ❓ Getting Help

- Open a GitHub Issue for questions or discussions
- Check existing discussions and documentation first