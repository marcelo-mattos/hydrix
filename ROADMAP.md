# Hydrix Roadmap

This document outlines the planned evolution of **Hydrix**.
The roadmap reflects the project's core philosophy: **explicit SQL, predictable behavior, and minimal abstractions**.

Features are added deliberately, prioritizing correctness, transparency, and long-term maintainability over convenience or magic behavior.

---

## 🎯 Guiding Principles

- SQL remains explicit and fully visible
- No implicit query generation
- No hidden behaviors
- Performance and predictability over abstraction
- Provider-agnostic design based on ADO.NET
- Minimal and stable public API surface
- Zero reflection in the materialization hot path

---

## ✅ v1.0 — Foundation (Completed)

Focus: **explicit SQL, transparent mapping, and lightweight adoption**

### Delivered
- SQL-first ORM core for .NET
- Attribute-based metadata for entities, procedures, parameters, and commands
- Fluent `WhereBuilder` support for explicit filtering
- Provider-agnostic ADO.NET foundation
- Support for `netcoreapp3.1`, `net6.0`, `net8.0`, and `net10.0`

---

## ✅ v1.1 — Stabilization & Core Solidification (Completed)

Focus: **robustness, internal consistency, and metadata correctness**

### Delivered
- Thread-safe metadata caching
- Nested entity materialization
- SQL `IN` clause expansion
- Stored procedure mapping
- SQL command logging foundations
- Improved internal validations and test coverage

---

## ✅ v2.0 — Performance & Architectural Maturity (Completed)

Focus: **runtime efficiency and internal architecture**

### Delivered
- Removal of reflection from materialization hot paths
- Compiled property setters and factory delegates
- Compiled enum converters
- Improved metadata conflict validation
- Reduced allocations during nested materialization
- More deterministic and fail-fast mapping behavior

### Breaking Change
- Entities, procedures, and parameters migrated to the DataAnnotations-based attribute model.

---

## ✅ v2.1 — Core Evolution, Performance & Stability (Completed)

Focus: **execution architecture modernization, conversion correctness, and stronger runtime stability**

### Delivered
- `HydrixOptions` and centralized runtime configuration
- Dependency Injection integration via `AddHydrix(...)`
- `HydrixDataCore` extension-first usage paths
- Strongly typed stored procedure support via `IProcedure<TDataParameter>`
- Optional timeout support across execution and query APIs
- Process-wide hot cache refinements
- Broader benchmark and regression-testing foundations

---

## ✅ v3.0 — HydrixDataCore-Only Runtime (Completed)

Focus: **API simplification, migration closure, and higher throughput**

### Delivered
- Removal of the legacy `Materializer` and `IMaterializer` stacks
- `HydrixDataCore` established as the only supported execution/query entry point
- Additive Entity Framework interoperability through `HydrixEntityFramework.RegisterModel(...)`
- Startup and dependency-injection automation for Entity Framework model registration through `AddHydrixEntityFrameworkModel<TDbContext>()` and `UseHydrixEntityFrameworkModels()`
- Translation of `OnModelCreating` metadata into Hydrix's existing validation, query-building, and materialization caches
- Faster conversion pipeline with per `(sourceType, targetType)` converter caches
- Optimized conversions for numeric values, `DateTimeOffset`, `TimeSpan`, and boolean aliases
- Opt-in command logging via `HydrixOptions.EnableCommandLogging`
- Improved schema-binding concurrency in `TableMaterializeMetadata`
- Lower-overhead row materializer execution via direct delegate invocation and cached `MethodInfo`
- Lower-allocation command/procedure execution through closure-free parameter binding paths and shared default options
- Stronger fallback type matching with reduced boxing in provider edge cases
- Atomic hot-cache entries for converters and binders
- Expanded unit test coverage and XML documentation standardization

### Breaking Change
- Materializer-based usage was completely removed. Hydrix 3.x must be consumed through `HydrixDataCore`.

### Benchmark Snapshot

Against Dapper, the current benchmark suite shows:

- Flat / Take 1000: Hydrix `986.3 us` vs Dapper `1,062.0 us` (`7.1%` faster)
- Flat / Take 10000: Hydrix `10,362.5 us` vs Dapper `12,731.9 us` (`18.6%` faster)
- Nested / Take 1000: Hydrix `1.684 ms` vs Dapper `1.744 ms` (`3.4%` faster)
- Nested / Take 10000: Hydrix `17.805 ms` vs Dapper `21.089 ms` (`15.6%` faster)
- Flat allocations: `42.4%` less at `Take 1000`, `40.4%` less at `Take 10000`
- Nested allocations: `46.5%` less at `Take 1000`, `45.0%` less at `Take 10000`

Hydrix 3.0 reaches the release line with a mature nested pipeline, lower memory pressure, and benchmark results consistently above Dapper in the current suite.

---

## 🔜 v3.1 — Domain Decoupling, EF Coexistence & More Throughput (Planned)

Focus: **removing Hydrix contract requirements from domain types, deepening coexistence with existing enterprise stacks, and continuing hot-path gains**

### Planned
- Remove the requirement for domain entities to implement `Hydrix.Schemas.Contract` interfaces in Hydrix query/materialization flows
- Keep `ITable`, `IEntity`, and `IProcedure<TDataParameterDriver>` available as compatibility markers instead of mandatory domain contracts
- Allow plain CLR table and view types to participate in attribute-based and Entity Framework-backed metadata resolution
- Allow stored procedure descriptor types to execute without mandatory Hydrix contract interfaces when they expose valid Hydrix metadata
- Replace public API generic constraints that currently enforce Hydrix contracts with runtime validation based on metadata compatibility and constructor availability where required
- Refactor nested-materialization internals that currently rely on `ITable` so they operate on plain object graphs without changing the existing runtime behavior
- Expand Entity Framework interoperability so translated `OnModelCreating` metadata no longer depends on domain types implementing Hydrix contracts
- Preserve backward compatibility for applications that already implement Hydrix marker interfaces
- Add migration guidance and examples showing contract-free domain models for table, view, and procedure scenarios
- Continue nested-materialization tuning without compromising flat-query speed
- More provider-aware fast paths where they materially improve runtime behavior
- Broader benchmark scenarios and comparative tracking across ADO.NET, Dapper, and Entity Framework
- Additional diagnostics for migration and runtime observability

### Transition Strategy
- Phase 1: remove mandatory contract checks from public APIs while keeping the existing interfaces fully supported
- Phase 2: switch metadata resolution, validation, and nested materialization internals from contract markers to metadata-driven eligibility
- Phase 3: update docs, benchmarks, and migration guidance to position Hydrix as contract-optional for domain models

### Compatibility Goal
- Existing applications that already implement Hydrix contract interfaces should continue to work unchanged
- New or existing domain models should be able to use Hydrix without introducing mandatory dependencies on `Hydrix.Schemas.Contract`
---

## 🚀 v3.x+ — Post-3.0 Performance & Reliability Evolution (Planned)

Focus: **continuous throughput gains and operational stability**

### Direction
- Advanced batching and streaming strategies for large result sets
- Optional compile-time metadata generation where it clearly improves latency
- Faster cold-start and cache warm-up behavior
- Stronger stress and chaos testing for connection failures, timeouts, and retries
- Benchmark regression gates in CI/CD

---

## ❌ Explicitly Out of Scope

The following features are intentionally **not planned**:

- LINQ provider or expression-based query translation
- Automatic SQL generation
- Entity tracking or change detection
- Lazy loading proxies
- Implicit query execution
- Hidden caching mechanisms

Hydrix will remain a **SQL-first** and **developer-controlled** framework.

---

## 📊 Roadmap Philosophy

Hydrix evolves conservatively.

Every new feature must:

- Preserve explicit SQL
- Maintain predictable runtime behavior
- Avoid hidden abstractions
- Improve performance or correctness
- Not compromise long-term maintainability

---

## 🤝 Contributions & Feedback

Community feedback is welcome and helps guide the roadmap.
Feature requests should align with Hydrix’s guiding principles and avoid introducing hidden behaviors or excessive abstraction.

---

## 👨‍💻 Author

**Marcelo Matos dos Santos**
Software Engineer • Open Source Maintainer
