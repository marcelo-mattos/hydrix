# Changelog

All notable changes to Hydrix.Mapper will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] - 2026-04-06

🎉 **Initial public release of Hydrix.Mapper.**

Hydrix.Mapper 1.0.0 delivers a high-performance, zero-reflection object-to-DTO
projection library that outperforms AutoMapper across every measured scenario:
single-object mapping, collection mapping, type conversions, and memory
efficiency.

### ✨ Added

#### Hot Path Architecture

- Expression-tree-based plan compilation with a single fused delegate per type
  pair that creates the destination instance, transfers every mapped property,
  and returns — eliminating separate factory and mapper delegate invocations
  from the hot path.
- Typed local variables in compiled expressions so the source and destination
  casts are each emitted once, regardless of property count, reducing redundant
  `castclass` instructions in the generated IL.
- Typed compiled delegate (`Func<TSource, TTarget>`) per plan, used by the
  generic typed API to eliminate boundary casts from the hot path.
- Identity reference-type assignments skip the null-check wrapper — when no
  conversion is applied, direct assignment handles null naturally and the
  explicit `Condition` expression is omitted from the compiled IL.

#### Caching

- Per-instance fast cache keyed only by the source and destination type pair,
  eliminating option-key construction and the associated hash computation on
  every hot-path call.
- Global `MapPlanCache` keyed by source type, destination type, and option
  snapshot, ensuring plans are compiled at most once per unique type-pair and
  configuration combination across all mapper instances.

#### Conversion Support

- String transforms: `Trim`, `TrimStart`, `TrimEnd`, `Uppercase`, `Lowercase`,
  and bitwise combinations.
- Guid formatting: format specifiers `N`, `D`, `B`, `P` with `Lower`/`Upper`
  casing control.
- DateTime and DateTimeOffset to string: custom format string, timezone
  normalization (`None`, `ToUtc`, `ToLocal`), and culture-aware output.
- DateOnly to string (.NET 6+).
- Bool to string: eight built-in presets (`TrueFalse`, `LowerCase`, `YesNo`,
  `YN`, `OneZero`, `SN`, `SimNao`, `TF`) plus fully custom `TrueValue`/
  `FalseValue` strings.
- Decimal and float to integral: `Truncate`, `Ceiling`, `Floor`, `Nearest`,
  and `Banker` rounding strategies.
- Integer overflow control: `Throw`, `Clamp`, and `Truncate` strategies.
- Enum to string via `ToString()`.

#### API Surface

- `IHydrixMapper` interface with four overloads:
  - `Map<TTarget>(object)` — untyped, resolves plan using runtime source type.
  - `Map<TSource, TTarget>(TSource)` — typed, resolves plan using compile-time
    source type.
  - `MapList<TTarget>(IEnumerable<object>)` — untyped heterogeneous list with
    per-item plan resolution and last-type local cache.
  - `MapList<TSource, TTarget>(IEnumerable<TSource>)` — typed homogeneous list
    with `CollectionsMarshal.AsSpan` fast path on .NET 6+, `IList<T>` index
    loop fast path on all targets, and pre-sized result buffers.
- `AddHydrixMapper` dependency injection extension registering `IHydrixMapper`
  as a singleton with configurable options.
- `ToDto<TDest>()` and `ToDtoList<TDest>()` convenience extension methods
  routing through the default global mapper instance.

#### Plan Compilation Features

- `[MapConversion]` per-property override attribute, read exclusively at
  plan-compile time with zero runtime cost.
- `[NotMapped]` support — destination properties decorated with
  `System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute` are
  excluded from plan compilation.
- Strict mode — throws `InvalidOperationException` at plan-compile time when a
  destination property has no matching source property.
- Nullable source propagation — null guards are baked directly into the
  compiled expression tree.

#### Infrastructure

- Multi-targeted for `net10.0`, `net8.0`, `net6.0`, and `netcoreapp3.1` with
  conditional `ArgumentNullException.ThrowIfNull` on .NET 8 and above.
- `CollectionsMarshal.AsSpan` fast path gated by `#if NET6_0_OR_GREATER` for
  bounds-check-free list iteration.

### 📊 Performance

Benchmark snapshot against AutoMapper 12 — MediumRun, .NET 10.0.5, X64 RyuJIT AVX2:

| Scenario | Hydrix.Mapper | AutoMapper | Gain |
| --- | ---: | ---: | ---: |
| flat small (5 props) | **16 ns** | 33 ns | ~52% faster |
| flat medium (12 props) | **21 ns** | 38 ns | ~45% faster |
| flat large (20 props) | **23 ns** | 41 ns | ~45% faster |
| with conversions | **56 ns** | 72 ns | ~22% faster |
| list small ×100 | **751 ns** | 1,071 ns | ~30% faster |
| list medium ×100 | **1,449 ns** | 1,687 ns | ~14% faster |
| list large ×100 | **1,519 ns** | 2,083 ns | ~27% faster |
| list small ×1000 | **7,939 ns** | 9,834 ns | ~19% faster |
| list medium ×1000 | **13,932 ns** | 15,606 ns | ~11% faster |
| list large ×1000 | **14,846 ns** | 16,788 ns | ~12% faster |

Allocation snapshot against AutoMapper 12:

| Scenario | Hydrix.Mapper | AutoMapper | Reduction |
| --- | ---: | ---: | ---: |
| list small ×100 | **5,696 B** | 6,992 B | ~19% less |
| list medium ×100 | **12,096 B** | 13,392 B | ~10% less |
| list large ×100 | **16,096 B** | 17,392 B | ~7% less |
| list small ×1000 | **56,096 B** | 64,600 B | ~13% less |
| list medium ×1000 | **120,096 B** | 128,600 B | ~7% less |
| list large ×1000 | **160,096 B** | 168,600 B | ~5% less |

### 🧪 Tests

- 208 unit tests on .NET 10, .NET 8, and .NET 6.
- 204 unit tests on .NET Core 3.1.
- Coverage: 100% line · 100% branch · 100% method across all supported targets.
- Test surface covers all mapping paths, conversion scenarios, cache behavior,
  DI registration, collection fast paths, edge cases, and regression scenarios.

---

## [Unreleased]

### Added

- Nothing yet.

### Changed

- Nothing yet.

### Fixed

- Nothing yet.
