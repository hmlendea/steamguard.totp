# Privacy and Personal Data

This document describes the data-handling behaviour of the SteamGuard.TOTP library. The library is a local computation component with no network access, telemetry, or persistent storage. It processes a caller-supplied Base32 TOTP secret in memory to generate a Steam Guard authentication code and retains no data.

**Information reviewed:** 2026-10-11

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [Data Protection and Security](#data-protection-and-security)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how SteamGuard.TOTP at https://github.com/hmlendea/steamguard.totp handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

SteamGuard.TOTP is distributed as a NuGet package and runs entirely within the consuming application's process. The library maintainers do not operate any service, collect any data, or receive any information from deployed instances. Instance operators (application developers who reference the package) control all configuration, local storage, logs, backups, access controls, retention, and request handling for their applications. The library itself performs no network calls, telemetry, update checks, crash reporting, email, authentication, reverse-proxy, object-storage, or monitoring integrations.

## 📥 Data We Handle

### Data Provided to the Application

The library accepts a single string parameter (`totpKey`) representing a Base32-encoded TOTP shared secret. This secret is provided by the caller at runtime and is not requested, validated, or stored by the library.

### Data Generated or Collected by the Application

No personal data is generated or collected automatically. The library computes a 5-character authentication code from the supplied secret and returns it to the caller. No logs, metrics, crash reports, or telemetry are produced by the library.

### Data Received from Integrations

No personal data is received from integrations or third parties. The library has no built-in integrations.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Generate Steam Guard authentication code — Base32 TOTP secret provided by caller

## 🗄️ Storage, Retention, and Deletion

The library performs no persistent storage. The supplied secret and computed code exist only in managed memory during the `GenerateAuthenticationCode` call and are eligible for garbage collection immediately after the method returns. No databases, files, logs, caches, or backups are created by the library. For self-hosted deployments, the instance operator controls all storage, deletion, and backup behaviour of their application.

## 🔗 External Processing and Integrations

The application has no built-in external data transfer. The only external dependency is the `NuciExtensions` NuGet package (for `DateTimeExtensions.GetElapsedUnixTime`), which is a compile-time dependency and performs no network activity or data collection at runtime.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| NuciExtensions | Provides `DateTimeExtensions.GetElapsedUnixTime` for time-step calculation | None (compile-time only) | https://www.nuget.org/packages/NuciExtensions |

## 🛡️ Data Protection and Security

The library is a pure computation component with no network exposure, secrets management, or access controls. It uses `HMACSHA1` from `System.Security.Cryptography` for the TOTP algorithm. For self-hosted deployments, the instance operator is responsible for application-level security including dependency updates, secrets handling, access controls, backups, network exposure, and log protection. No absolute security is promised.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/steamguard.totp/blob/main/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers at https://github.com/hmlendea/steamguard.totp/issues. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the library version and deployment context if relevant; do not send passwords, access tokens, or other secrets.