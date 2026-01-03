# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.1.1] - 2026-01-03

This release focuses on correctness and performance improvements in the
entity materialization pipeline, addressing edge cases involving `DBNull`
handling and removing reflection from runtime execution paths.
No breaking changes were introduced.

### 🐞 Fixed

- Correct handling of `DBNull` values for non-nullable value types during
  entity materialization, preventing runtime assignment exceptions.
- Ensured default values for value types are safely applied when database
  fields contain `NULL`.

### 🚀 Improved

- Eliminated reflection from hot paths in entity materialization by caching
  default values and compiled setters in metadata.
- Improved overall performance and predictability of SQL-to-entity mapping
  without altering the public API.

---

## [1.1.0] - 2025-12-24

This release represents a significant evolution of Hydrix, focusing on
performance, explicit SQL control, and internal architecture improvements,
while preserving a stable and predictable public API.

### ✨ Added

- **Minimal SQL-first ORM Core**  
  Introduced the foundational ORM core for .NET, providing explicit control
  over SQL execution, predictable behavior, and transparent entity materialization.

- **Metadata caching for entity materialization**  
  Implemented centralized metadata caching to eliminate repeated reflection
  during entity hydration. Reflection is now executed once per entity type,
  with compiled delegates used in hot paths for optimal performance.

- **Nested entity materialization support**  
  Enhanced support for mapping nested entities from flattened SQL projections,
  including conditional instantiation based on primary key presence and
  recursive object graph construction.

- **ExecuteNonQuery overloads**  
  Added new `ExecuteNonQuery(string, object)` overloads with support for external
  `DbTransaction` usage, improving transaction control and integration scenarios.

### 🚀 Improved

- **SqlWhereBuilder**  
  Expanded the `SqlWhereBuilder` API with more expressive and flexible options
  for constructing complex WHERE clauses while maintaining explicit SQL semantics.

- **Entity mapping performance**  
  Refactored internal mapping logic to remove reflection from runtime execution
  paths, replacing it with cached metadata and compiled setters and factories.

### 📝 Documentation

- **README.md**  
  Updated to reflect the current feature set and clarify Hydrix’s design goals,
  usage patterns, and philosophy.

- **ROADMAP.md**  
  Added a roadmap outlining planned features and architectural directions
  for future releases.

### 🛠️ Maintenance

- **FUNDING.yml**  
  Added funding configuration to enable sponsorship and long-term project support.

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
- (Planned) Diagnostic helpers for SQL inspection and debugging.

### Changed
- (Planned) Performance optimizations for high-throughput scenarios.

### Fixed
- (Planned) Minor internal refactorings based on community feedback.

