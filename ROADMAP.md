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

## ✅ v1.0 — Stabilization & Refinement (Completed)

Focus: **robustness, performance, and developer experience**  
_No breaking changes._

### Delivered
- Metadata caching optimizations to further reduce reflection overhead
- Improved diagnostics and debugging helpers for generated SQL
- Additional guardrails and validations for SQL builders
- Minor performance optimizations in entity materialization
- Improved XML documentation and code comments
- Expanded test coverage, including edge cases for nested entities

---

## 🔄 v1.1 — Stabilization & Core Solidification (Completed)

Focus: **robustness, internal consistency, and metadata correctness**  
_No breaking changes._

### Delivered
- Thread-safe metadata caching
- Nested entity materialization
- SQL `IN` clause expansion
- Stored procedure mapping
- SQL command logging
- Guardrails for SQL builder statefulness
- Improved internal validations
- Expanded test coverage

---

## 🚀 v2.0 — Performance & Architectural Maturity (Completed)

Focus: **runtime efficiency and internal architecture**  
_**Breaking change**: consumers must migrate entities, procedures, and parameters to the new DataAnnotations-based attribute model._

### Delivered
- Removal of reflection from materialization hot paths
- Compiled property setters and factory delegates
- Compiled enum converters (no Enum.ToObject per row)
- Improved metadata conflict validation
- Fail-fast detection for invalid attribute combinations
- Optimized nested entity resolution
- Reduced allocations during materialization
- Improved internal caching structure
- Refined error messages and mapping validation

Hydrix 2.0 establishes a stable, performance-oriented foundation for long-term evolution.

---

## 🔬 v2.1 — Core Evolution, Performance & Stability (Completed)

Focus: **execution architecture modernization, conversion correctness, and stronger runtime stability**

### Delivered
- Hydrix configuration model with centralized runtime options (`HydrixOptions`)
- Dependency Injection integration via `AddHydrix(...)`
- `HydrixDataCore` and extension-first usage paths
- Strongly typed stored procedure support via `IProcedure<TDataParameter>`
- Optional timeout support across execution and query APIs
- Materialization and execution pipeline refactoring (`CommandEngine`/`ParameterEngine` split)
- Conversion flow improvements (`As<T>`, `Guid`, provider `DbType` handling)
- Process-wide hot cache refinements and cache architecture improvements
- Expanded unit test coverage and validation hardening
- Benchmarking foundation for regression tracking

Hydrix 2.1 consolidates the transition to a more modular and performance-oriented runtime model.

---

## 🧩 v2.2 — Transitional Performance Release (Planned)

Focus: **higher throughput, safer migration, and legacy bridge stabilization**

### Planned

- Keep the legacy `Materializer` API available as `[Obsolete]` (migration bridge)
- Prioritize `HydrixDataCore` extension-based API in documentation and examples
- Add migration diagnostics/warnings to help identify remaining legacy API usage
- Improve async execution throughput and reduce allocations in query/command pipelines
- Expand hot-cache invalidation and reuse strategies for metadata/materialization internals
- Add more microbenchmarks for high-volume reads, joins, and scalar/non-query workloads
- Improve resilience under concurrent load and long-running process scenarios
- Refine error classification/messages for faster troubleshooting in production
- Increase regression and stress test coverage focused on performance and stability

#### All enhancements will preserve:

- Explicit SQL
- No hidden behavior
- No implicit query generation

---

## 🏗 v3.0 (LTS) — HydrixDataCore-Only Runtime (Planned)

Focus: **long-term support baseline with maximum runtime efficiency and API stability**

### Planned (Breaking Changes)

- Remove legacy `Materializer` API completely
- Adopt `HydrixDataCore` extension-based API as the only supported access model
- Finalize and simplify public API surface around extension-first contracts
- Remove compatibility layers kept only for 2.x transition support

### LTS Goals

- Strong backward stability guarantees within the 3.x LTS line
- Lower steady-state memory footprint and reduced GC pressure
- Predictable latency under concurrent workloads
- Stable diagnostics and observability primitives for production operations

Any breaking changes will strictly follow semantic versioning.

---

## 🚀 v3.x+ — Post-LTS Performance & Reliability Evolution (Planned)

Focus: **continuous throughput gains and operational stability**

### Direction

- Adaptive command/materialization pipelines tuned by workload profile
- Deeper provider-specific fast paths (without compromising provider-agnostic defaults)
- Advanced batching and streaming strategies for large result sets
- Optional compile-time metadata generation where it clearly improves hot-path latency
- Additional safeguards for cold-start determinism and cache warm-up behavior
- Expanded chaos/stress testing for connection failures, timeouts, and retries
- Performance regression gates in CI/CD using benchmark baselines

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
