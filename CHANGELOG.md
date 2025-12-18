# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] - 2025-12-17

🎉 **Initial public release**

### Added
- SQL-first data hydration engine with explicit and predictable behavior.
- Fluent SQL builders for composing `WHERE` clauses with support for:
  - `AND`, `OR`, `NOT`
  - grouped conditions
  - dynamic `IN` clauses with parameter binding.
- Attribute-based metadata for commands, entities, parameters, and procedures.
- Safe and explicit parameter binding to prevent SQL injection.
- Lightweight object materialization using reflection.
- Support for multiple target frameworks:
  - `netcoreapp3.1`
  - `net6.0`
  - `net8.0`
  - `net10.0`
- Zero external runtime dependencies.
- Full open-source readiness:
  - Apache License 2.0
  - README, LICENSE, NOTICE, CONTRIBUTING, and CODE_OF_CONDUCT files.

### Changed
- N/A (initial release).

### Fixed
- N/A (initial release).

### Removed
- N/A (initial release).

---

## [Unreleased]

### Added
- (Planned) Metadata caching to reduce reflection overhead.
- (Planned) Diagnostic helpers for SQL inspection and debugging.

### Changed
- (Planned) Performance optimizations for high-throughput scenarios.

### Fixed
- (Planned) Minor internal refactorings based on community feedback.

