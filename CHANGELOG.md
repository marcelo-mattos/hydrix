# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [3.0.0] - 2026-04-04

Hydrix 3.0.0 finalizes the transition to a **HydrixDataCore-only** runtime,
removes the legacy `Materializer` stack, strengthens the conversion and
materialization pipelines, and ships with benchmark results that place Hydrix
above Dapper in both flat and nested scenarios.

### ✨ Added

- Opt-in command logging control through `HydrixOptions.EnableCommandLogging`.
- Additive Entity Framework interoperability through
  `Hydrix.EntityFramework.HydrixEntityFramework.RegisterModel(...)`, allowing
  Hydrix to translate `DbContext` / `Model` metadata into the existing
  query-building, validation, and materialization caches.
- Startup and dependency-injection automation for Entity Framework model
  registration through `AddHydrixEntityFrameworkModel<TDbContext>()` and
  `UseHydrixEntityFrameworkModels()`.
- Faster conversion paths in `ObjectExtensions` for numeric values,
  `DateTimeOffset`, `TimeSpan`, and extended boolean aliases.
- Converter caching by `(sourceType, targetType)` to avoid repeated runtime
  resolution and reduce conversion overhead.
- Stronger schema-binding concurrency in `TableMaterializeMetadata`, including
  `Lazy<T>`-backed initialization and safer binding replacement behavior.
- Expanded test coverage for logging, conversion, provider edge cases,
  concurrency, materialization fallbacks, and Entity Framework interoperability.

### 🔄 Changed

- `HydrixDataCore` is now the single supported entry point for command,
  scalar, reader, and query execution.
- Legacy `Materializer` and `IMaterializer` paths were removed from the
  runtime and test surface.
- Entity Framework model translation now coexists with the existing
  attribute-based pipeline instead of replacing it, so projects can register
  `OnModelCreating` metadata without changing Hydrix's internal structures.
- Row materializer construction now favors direct delegate invocation and
  cached `MethodInfo`, reducing indirection and improving JIT inlining.
- Command and procedure parameter-binding paths were refactored to avoid
  per-call closure allocations.
- Process-wide hot caches for binders and converters now use atomic immutable
  entries, improving formal concurrency correctness.
- Type matching in binding resolution avoids unnecessary boxing when provider
  metadata is incomplete or unavailable.
- Conversion fallback dispatch no longer depends on `data.GetType()` for the
  known hot-path runtime types.
- Limit-based loops in `DataReaderExtensions` were tightened to avoid extra
  iterations during bounded reads.
- Internal architecture was simplified around the new execution model, with
  obsolete orchestration paths removed and runtime responsibilities clarified.

### ❗ Breaking Changes

- `Materializer`, `IMaterializer`, and all Materializer-based APIs were
  completely removed.
- Starting with `3.0.0`, Hydrix must be used through the `HydrixDataCore`
  extension-based API.
- Applications still using legacy Materializer flows must migrate to
  `Execute`, `ExecuteScalar`, `Query`, `QueryFirst`, `QuerySingle`, and the
  related `HydrixDataCore` surface.

### 📊 Performance Improvements

Current benchmark snapshot against Dapper:

| Scenario | Hydrix | Dapper | Hydrix Gain |
| --- | ---: | ---: | ---: |
| Flat / Take 1000 | 986.3 us | 1,062.0 us | 7.1% faster |
| Flat / Take 10000 | 10,362.5 us | 12,731.9 us | 18.6% faster |
| Nested / Take 1000 | 1.684 ms | 1.744 ms | 3.4% faster |
| Nested / Take 10000 | 17.805 ms | 21.089 ms | 15.6% faster |

Allocation snapshot against Dapper:

| Scenario | Hydrix | Dapper | Hydrix Reduction |
| --- | ---: | ---: | ---: |
| Flat / Take 1000 | 95.77 KB | 166.40 KB | 42.4% less |
| Flat / Take 10000 | 1039.01 KB | 1742.63 KB | 40.4% less |
| Nested / Take 1000 | 135.24 KB | 252.84 KB | 46.5% less |
| Nested / Take 10000 | 1430.18 KB | 2602.50 KB | 45.0% less |

### 🛠 Fixed

- Reduced fallback overhead in binding/type resolution for providers that do
  not expose full metadata.
- Improved stability of concurrent schema-binding initialization paths.
- Fixed hot-cache consistency windows by storing converter and binder cache
  pairs atomically.
- Table entities with no materializable members now fail fast instead of
  proceeding with a silent invalid contract.
- Tightened command logging behavior so logging is explicitly opt-in.

### 🧪 Tests

- Expanded regression coverage across conversion, logging, binding plans,
  provider-specific compatibility paths, and row-materialization internals.

### 🧹 Maintenance

- Standardized XML documentation across public, internal, and private members.
- Removed obsolete files and tests related to the legacy Materializer runtime.
- Simplified the codebase around the post-`Materializer` architecture.

---

## [2.1.0] - 2026-03-19

Hydrix 2.1.0 expands the 2.0 foundation with stronger execution architecture,
better dependency injection/configuration support, improved conversion behavior,
and broader runtime coverage for reliability and performance.

### ✨ Added

- Configuration APIs for Hydrix setup.
- Dependency Injection integration and extension APIs.
- `HydrixDataCore` and related mapping extension points.
- Strongly typed stored procedure support via `IProcedure<T>`.
- Optional timeout parameter support across materializer execution methods.
- `MaterializerCache` for reusing materialization internals.
- `IntExtensions` and additional conversion helpers.
- Benchmarking suite for measuring runtime behavior.

### 🔄 Changed

- Internal command and parameter responsibilities extracted to
  `CommandEngine` and `ParameterEngine`.
- Command execution and materialization flow centralized and refactored.
- Mapping pipeline improved with async path refinements and lower overhead.
- `As<T>` conversion flow revised for more predictable coercion behavior.
- Provider `DbType` setter behavior refined, including `Guid` handling.
- Cache architecture migrated to a process-wide volatile hot cache model.

### 🛠 Fixed

- Core conversion edge cases affecting default value and type coercion paths.
- `Guid` conversion inconsistencies across providers.
- Validation hardening for metadata/materialization scenarios.
- Stability issues discovered during expanded unit test coverage.

### 🧪 Tests

- Increased unit test coverage across orchestration, caching, conversion,
  materialization, and validation flows.

### 🧹 Maintenance

- Removed obsolete `report.coverage` artifact from repository history.
- Version/package metadata aligned for the `2.1.0` release.

---

## [2.0.0] - 2026-02-28

Hydrix 2.0 introduces significant internal architectural improvements focused on:

- Runtime performance
- Deterministic behavior
- Strict metadata validation
- Elimination of reflection in hot paths

This version establishes a stable and performance-oriented foundation for long-term evolution.

### ✨ Added

#### Performance & Architecture

- Compiled property setters using expression trees
- Compiled entity factory delegates
- Compiled enum converters (eliminated `Enum.ToObject` from hot path)
- Removal of reflection during row materialization
- Improved metadata caching architecture
- Reduced memory allocations in nested entity resolution

#### Validation & Safety

- Fail-fast validation for conflicting attributes (`[Column]` + `[ForeignTable]`)
- Stricter metadata separation between column mappings and nested entities
- Earlier detection of invalid entity configurations
- Improved default value handling for non-nullable types

#### Internal Improvements

- Thread-safe cache refinements
- Cleaner separation between metadata building and materialization
- Improved enum handling pipeline
- Reduced delegate recompilation
- Improved internal structure of `FieldReaderFactory`

### 🔄 Changed

#### Enum Conversion

- Replaced `Enum.ToObject` with compiled converters
- Enum conversion now uses cached delegates per enum type
- Improved performance in enum-heavy scenarios

#### Nested Entity Materialization

- Nested properties marked with `[ForeignTable]` are no longer treated as column fields
- Metadata now guarantees mutual exclusivity between fields and nested entities
- More predictable behavior in LEFT JOIN scenarios

#### Default Value Handling

- Non-nullable value types now use cached default value factories
- Improved consistency across providers when handling `DBNull`

#### Metadata Building

- Metadata generation logic refactored for clarity and correctness
- Reduced repeated reflection calls during metadata creation

### ❗ Breaking Changes

- Consumers must migrate entities, procedures, and
parameters to the new DataAnnotations-based attribute model.

### 🛠 Fixed

- Resolved incorrect materialization when nested properties were treated as fields
- Eliminated reflection usage in materialization hot path
- Reduced unnecessary allocations during enum conversion
- Improved null handling consistency for nested entities

### 📊 Performance Improvements

- Removed reflection from row materialization
- Removed Enum.ToObject from hot path
- Reduced delegate creation frequency
- Reduced allocations during nested resolution
- Improved internal caching efficiency

Hydrix 2.0 is optimized for high-throughput and performance-critical scenarios.

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

- **WhereBuilder**
  Expanded the `WhereBuilder` API with more expressive and flexible options
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
- No entries yet.

### Changed
- Planned provider-aware performance work for nested materialization, schema binding,
  and broader EF coexistence scenarios.

### Fixed
- Planned internal refactorings and diagnostics improvements based on release feedback.
