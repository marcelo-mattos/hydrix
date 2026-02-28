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

## 🔬 v2.1 — Performance & Diagnostics Enhancements

Focus: measurement, profiling, and observability

### Planned
- Official benchmark suite (Hydrix vs Dapper vs ADO.NET baseline)
- Allocation profiling scenarios
- Optional performance diagnostics hooks
- Improved logging extensibility
- Better structured error messages for mapping failures
- Enhanced debug tooling for metadata inspection

---

## 🧩 v2.2 — Advanced Scenarios (Non-Magical Extensibility)

Focus: controlled extensibility

### Planned

- Public extension points for custom materialization strategies
- Pluggable naming conventions
- Optional advanced provider-specific optimizations
- More granular control over nested materialization behavior
- Additional stored procedure patterns

#### All enhancements will preserve:

- Explicit SQL
- No hidden behavior
- No implicit query generation

---

## 🏗 v3.0 — Long-Term Evolution (Under Evaluation)

Focus: **carefully evaluated expansion**

Possible areas of exploration (not commitments):

- Advanced batching scenarios
- Further GC pressure reduction strategies
- Optional source generators for metadata (if aligned with philosophy)
- Additional compile-time validation helpers

Any breaking changes will strictly follow semantic versioning.

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
