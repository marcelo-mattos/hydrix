# Hydrix.Mapper Roadmap

This document outlines the planned evolution of **Hydrix.Mapper**.
The roadmap reflects the project's core philosophy: **compile once, zero reflection, predictable performance**.

Features are added deliberately, prioritizing correctness, performance, and long-term maintainability over convenience or magic behavior.

---

## 🎯 Guiding Principles

- Zero reflection on the hot path
- Plans are compiled once and cached permanently
- Explicit conversion rules — no hidden inference
- Performance and predictability over abstraction
- Thread-safe by design — no locks on the hot path
- Minimal and stable public API surface
- No non-Microsoft runtime dependencies

---

## ✅ v1.0 — Foundation (Completed)

Focus: **expression-tree compilation, conversion pipeline, and benchmark parity against AutoMapper**

### Delivered

- Expression-tree-based plan compilation with a single fused delegate per type pair
- Typed local variables in compiled expressions to emit each cast once regardless of property count
- Typed compiled delegate (`Func<TSource, TTarget>`) per plan for the generic typed API
- Identity reference-type assignment optimization — null-check wrappers omitted when no conversion is applied
- Per-instance fast cache keyed by `(Type source, Type dest)` pair
- Global `MapPlanCache` keyed by source type, destination type, and option snapshot
- `CollectionsMarshal.AsSpan` fast path for typed list mapping on .NET 6+
- `IList<T>` index-loop fast path for typed list mapping on all targets
- Last-type local cache in the untyped `MapList<TTarget>` overload for heterogeneous lists
- Pre-sized result buffers using `ICollection<T>`, `IReadOnlyCollection<T>`, and `ICollection` count
- Full conversion suite: string transforms, Guid formatting, DateTime/DateTimeOffset/DateOnly to string, bool to string, decimal/float to integral, integer overflow handling, enum to string
- Per-property `[MapConversion]` attribute override — zero runtime cost
- `[NotMapped]` attribute support
- Strict mode — throws at plan-compile time on unmatched destination properties
- Nullable source propagation — null guards baked into expression trees
- `AddHydrixMapper` DI extension
- `ToDto<TDest>()` and `ToDtoList<TDest>()` convenience extension methods
- Multi-targeted: `net10.0`, `net8.0`, `net6.0`, `netcoreapp3.1`
- 247/243 unit tests with 100% line/branch/method coverage across all targets
- Benchmark suite comparing Hydrix.Mapper against AutoMapper 12

### Benchmark Results

Hydrix.Mapper 1.0.0 outperforms AutoMapper 12 across every measured scenario (LongRun, 100 iterations, 3 launches):

- Flat small (5 props): **~51% faster**
- Flat medium (12 props): **~44% faster**
- Flat large (20 props): **~42% faster**
- With conversions: **~27% faster**
- List small ×100: **~33% faster**, ~19% fewer allocations
- List large ×1000: **~12% faster**, ~5% fewer allocations

---

## 🔜 v1.1 — Refinement & Usability (Planned)

Focus: **allocation reduction, diagnostic tooling, and API ergonomics**

### Planned

- Reduce the flat small single-object allocation gap (88 B Hydrix vs 48 B AutoMapper) — investigate destination object header and field layout as the dominant cost
- `MapPlanCache` growth safeguard — optional bounded cache with LRU eviction for processes with highly dynamic type pairs
- `HydrixMapperOptions.Clone()` — explicit snapshot copying for multi-configuration scenarios
- Diagnostic API — `IHydrixMapper.IsCached(Type source, Type dest)` for introspection and startup validation
- XML documentation completion for edge-case enum variants and conversion attribute fields

---

## 🚀 v1.x+ — Post-1.0 Performance Evolution (Planned)

Focus: **JIT-friendly execution, source-generator integration, and struct source support**

### Direction

- Source-generator companion (`Hydrix.Mapper.Generator`) — emit strongly typed plan delegates at compile time, eliminating expression-tree compilation from the cold path entirely
- Struct source support — explicit handling of value-type sources to avoid boxing in `Map<TSource, TTarget>(TSource)` when `TSource` is a struct
- Per-type-pair result buffer pooling — `ArrayPool<T>`-based scratch buffers for very large list mappings
- Benchmark regression gates in CI/CD to detect and block hot-path regressions before merge

---

## ❌ Explicitly Out of Scope

The following features are intentionally **not planned**:

- LINQ provider or expression-based query translation
- Deep-graph or recursive object mapping
- Runtime reconfiguration of compiled plans
- Convention-based property name inference beyond exact name matching
- AutoMapper profile compatibility layer
- Reflection-based fallback paths

Hydrix.Mapper will remain a **compile-once, zero-reflection** mapper.

---

## 📊 Roadmap Philosophy

Hydrix.Mapper evolves conservatively.

Every new feature must:

- Preserve zero reflection on the hot path
- Maintain or improve benchmark results
- Not increase cold-path overhead for typical type pairs
- Have full unit test coverage before merge
- Not compromise long-term maintainability

---

## 🤝 Contributions & Feedback

Community feedback is welcome and helps guide the roadmap.
Feature requests should align with Hydrix.Mapper's guiding principles and avoid introducing hidden behaviors, convention magic, or reflection on the hot path.

---

## 👨‍💻 Author

**Marcelo Matos dos Santos**
Software Engineer • Open Source Maintainer
