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
- Guid formatting: `Hyphenated`, `DigitsOnly`, `Braces`, `Parentheses` with
  `Lower`/`Upper` casing control.
- DateTime and DateTimeOffset to string: custom format string, timezone
  normalization (`None`, `ToUtc`, `ToLocal`), and culture-aware output.
- DateOnly to string (.NET 6+).
- Bool to string: six built-in presets (`TrueOrFalse`, `LowercaseTrueOrFalse`,
  `YesOrNo`, `YOrN`, `OneOrZero`, `TOrF`) plus `Custom` with explicit
  `TrueValue`/`FalseValue` strings.
- Decimal and float to integral: `Truncate`, `Ceiling`, `Floor`, `Nearest`,
  and `Banker` rounding strategies.
- Integer overflow control: `Throw`, `Clamp`, and `Truncate` strategies.
- Enum mapping: `AsString` (textual name) or `AsInt` (underlying integer) via
  the `EnumMapping` enum.

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

Benchmark snapshot against AutoMapper (`12.0.1` on `netcoreapp3.1`; `13.0.1` on `net6.0`, `net8.0`, and `net10.0`) using BenchmarkDotNet (`0.14.0` on `netcoreapp3.1` and `net6.0`; `0.15.8` on `net8.0` and `net10.0`) — LongRun (100 iterations, 3 launches, 15 warmups), host runtime `.NET 10.0.5`, `X64 RyuJIT AVX2`:

| Scenario | Hydrix.Mapper | AutoMapper | Gain |
| --- | ---: | ---: | ---: |
| flat small (5 props) | **18 ns** | 37 ns | ~51% faster |
| flat medium (12 props) | **26 ns** | 47 ns | ~44% faster |
| flat large (20 props) | **28 ns** | 48 ns | ~42% faster |
| with conversions | **66 ns** | 89 ns | ~27% faster |
| list small ×100 | **893 ns** | 1,324 ns | ~33% faster |
| list medium ×100 | **1,665 ns** | 2,143 ns | ~22% faster |
| list large ×100 | **1,795 ns** | 2,304 ns | ~22% faster |
| list small ×1000 | **9,417 ns** | 11,293 ns | ~17% faster |
| list medium ×1000 | **16,860 ns** | 18,607 ns | ~9% faster |
| list large ×1000 | **18,220 ns** | 20,610 ns | ~12% faster |

Allocation snapshot against the same AutoMapper baselines:

| Scenario | Hydrix.Mapper | AutoMapper | Reduction |
| --- | ---: | ---: | ---: |
| list small ×100 | **5,696 B** | 6,992 B | ~19% less |
| list medium ×100 | **12,096 B** | 13,392 B | ~10% less |
| list large ×100 | **16,096 B** | 17,392 B | ~7% less |
| list small ×1000 | **56,096 B** | 64,600 B | ~13% less |
| list medium ×1000 | **120,096 B** | 128,600 B | ~7% less |
| list large ×1000 | **160,096 B** | 168,600 B | ~5% less |

### 🧪 Tests

- 247 unit tests on .NET 10, .NET 8, and .NET 6.
- 243 unit tests on .NET Core 3.1.
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
