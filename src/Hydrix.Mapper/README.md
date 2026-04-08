# Hydrix.Mapper

![NuGet](https://img.shields.io/nuget/v/Hydrix.Mapper)
![NuGet Downloads](https://img.shields.io/nuget/dt/Hydrix.Mapper)
![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=marcelo-mattos_hydrix&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=marcelo-mattos_hydrix)

⚡ **A high-performance, zero-reflection object mapper for .NET.**

Hydrix.Mapper is an **object-to-DTO projection library** built for developers who demand:

- Zero reflection on the hot path
- Predictable, per-property compiled behavior
- Performance that surpasses AutoMapper across every scenario
- Full conversion control without custom profiles

Starting with **Hydrix.Mapper 1.0.0**, every mapping plan is compiled once via expression trees, cached permanently, and executed through a single fused delegate that creates the destination, transfers every property, and returns — with no per-call reflection, no hidden allocations, and no delegate overhead beyond the plan call itself.

---

## 🧭 Why Hydrix.Mapper?

Hydrix.Mapper is designed for systems where:

- DTO projection happens on every request and latency matters
- Teams need explicit, auditable per-property conversion rules
- Configuration must be done once, at startup, with zero runtime cost
- Thread safety is required without locks on the hot path

Hydrix.Mapper does not attempt to infer your mapping intent from conventions alone. You configure it, it compiles it, and it runs it fast.

---

## ⚠️ What Hydrix.Mapper is not

- A query language or LINQ provider
- A deep-graph serializer or recursive mapper
- A runtime-configurable mapper (plans are compiled at first use and are immutable)
- A replacement for AutoMapper in projects that rely on AutoMapper's profile system

---

## ⚙️ Supported frameworks

- .NET Core 3.1
- .NET 6
- .NET 8
- .NET 10

---

## ✨ Key Features

- Single fused compiled delegate per type pair — destination construction and all property transfers in one expression block
- Per-instance fast cache keyed by `(Type source, Type dest)` — eliminates option-key construction on every hot-path call
- Typed local variables in compiled expressions — each cast is emitted once regardless of property count
- Identity reference-type assignments skip the null-check wrapper — direct assignment handles null naturally
- String transforms: `Trim`, `TrimStart`, `TrimEnd`, `Uppercase`, `Lowercase`, and combinations
- Guid formatting: `Hyphenated`, `DigitsOnly`, `Braces`, `Parentheses` with `Lower`/`Upper` casing control
- DateTime and DateTimeOffset to string: custom format, timezone normalization (`None`, `ToUtc`, `ToLocal`), and culture
- DateOnly to string (.NET 6+)
- Decimal and float to integral: `Truncate`, `Ceiling`, `Floor`, `Nearest`, `Banker` rounding
- Integer overflow control: `Throw`, `Clamp`, `Truncate`
- Bool to string: six built-in presets (`TrueOrFalse`, `LowercaseTrueOrFalse`, `YesOrNo`, `YOrN`, `OneOrZero`, `TOrF`) plus `Custom` with explicit `TrueValue`/`FalseValue` strings
- Enum mapping: `AsString` (textual name) or `AsInt` (underlying integer) via `EnumMapping`
- Per-property override via `[MapConversion]` attribute — read only at cold path, zero runtime cost
- `[NotMapped]` support — respects `System.ComponentModel.DataAnnotations.Schema`
- Strict mode — throws on unmatched destination properties
- Nullable source propagation — null guards baked into expression trees
- `AddHydrixMapper` DI extension — registers `IHydrixMapper` as singleton
- Convenience extension methods: `ToDto<TDest>()`, `ToDtoList<TDest>()`
- No non-Microsoft runtime dependencies
- Apache 2.0 licensed

---

## 📊 Benchmark Snapshot vs AutoMapper

The benchmark suite compares Hydrix.Mapper against AutoMapper across flat-object widths, collection sizes, type conversion scenarios, and cold-path plan compilation.

Baseline versions by target framework:

- `netcoreapp3.1` — AutoMapper `12.0.1`
- `net6.0`, `net8.0`, `net10.0` — AutoMapper `13.0.1`

Environment:

- BenchmarkDotNet `0.14.0` on `netcoreapp3.1` and `net6.0`
- BenchmarkDotNet `0.15.8` on `net8.0` and `net10.0`
- Host runtime for the published snapshot: `.NET 10.0.5` · `X64 RyuJIT AVX2`
- Job: `LongRun` (100 iterations, 3 launches, 15 warmups)

### Single object — flat

| Scenario | Hydrix.Mapper | AutoMapper | Gain |
| --- | ---: | ---: | ---: |
| flat small (5 props) | **18 ns** | 37 ns | **~51% faster** |
| flat medium (12 props) | **26 ns** | 47 ns | **~44% faster** |
| flat large (20 props) | **28 ns** | 48 ns | **~42% faster** |

### Single object — with conversions

| Scenario | Hydrix.Mapper | AutoMapper | Gain |
| --- | ---: | ---: | ---: |
| string trim + guid + datetime + decimal→int | **66 ns** | 89 ns | **~27% faster** |

### Collections — speed

| Scenario | Hydrix.Mapper | AutoMapper | Gain |
| --- | ---: | ---: | ---: |
| list small ×100 | **893 ns** | 1,324 ns | **~33% faster** |
| list medium ×100 | **1,665 ns** | 2,143 ns | **~22% faster** |
| list large ×100 | **1,795 ns** | 2,304 ns | **~22% faster** |
| list small ×1000 | **9,417 ns** | 11,293 ns | **~17% faster** |
| list medium ×1000 | **16,860 ns** | 18,607 ns | **~9% faster** |
| list large ×1000 | **18,220 ns** | 20,610 ns | **~12% faster** |

### Collections — allocations

| Scenario | Hydrix.Mapper | AutoMapper | Reduction |
| --- | ---: | ---: | ---: |
| list small ×100 | **5,696 B** | 6,992 B | **~19% less** |
| list medium ×100 | **12,096 B** | 13,392 B | **~10% less** |
| list large ×100 | **16,096 B** | 17,392 B | **~7% less** |
| list small ×1000 | **56,096 B** | 64,600 B | **~13% less** |
| list medium ×1000 | **120,096 B** | 128,600 B | **~7% less** |
| list large ×1000 | **160,096 B** | 168,600 B | **~5% less** |

### Cold path

| Scenario | Hydrix.Mapper |
| --- | ---: |
| first hit (plan compile + execute) | **~453 ns** |

The cold path cost is paid exactly once per type pair per application lifetime. Every subsequent call uses the cached compiled plan with no reflection.

> Benchmark results are updated with each release. Run `benchmark.ps1` locally for your hardware profile.

---

## 📦 Installation

```bash
dotnet add package Hydrix.Mapper
```

---

## 🚀 Basic Usage

### Map a single object

```csharp
var mapper = new HydrixMapper(new HydrixMapperOptions());

var dto = mapper.Map<UserDto>(user);
```

### Map with compile-time source type

```csharp
var dto = mapper.Map<User, UserDto>(user);
```

### Map a list

```csharp
// Typed — single plan resolved once before the loop
IReadOnlyList<UserDto> dtos = mapper.MapList<User, UserDto>(users);

// Untyped — resolves plan per unique source runtime type
IReadOnlyList<UserDto> dtos = mapper.MapList<UserDto>(sources);
```

### Extension methods

```csharp
using Hydrix.Mapper.Extensions;

var dto = user.ToDto<UserDto>();

IReadOnlyList<UserDto> dtos = users.ToDtoList<User, UserDto>();
```

---

## 🧩 Configuration & DI

### Standalone

```csharp
var options = new HydrixMapperOptions();
options.String.Transform = StringTransforms.Trim;
options.Guid.Format     = GuidFormat.Hyphenated;
options.Guid.Case       = GuidCase.Lower;

var mapper = new HydrixMapper(options);
```

### Dependency injection

```csharp
using Hydrix.Mapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

services.AddHydrixMapper(options =>
{
    options.String.Transform              = StringTransforms.Trim;
    options.Guid.Format                   = GuidFormat.Hyphenated;
    options.Guid.Case                     = GuidCase.Lower;
    options.DateTime.StringFormat         = "O";
    options.DateTime.TimeZone             = DateTimeZone.None;
    options.Numeric.DecimalToIntRounding  = NumericRounding.Truncate;
    options.Numeric.Overflow              = NumericOverflow.Clamp;
    options.Bool.StringFormat             = BoolStringFormat.LowercaseTrueOrFalse;
});
```

`IHydrixMapper` is registered as a singleton. Inject it wherever you need projection:

```csharp
public class UserService(IHydrixMapper mapper)
{
    public UserDto GetUser(User user) => mapper.Map<User, UserDto>(user);
}
```

---

## 🔄 Conversion Options

### String transforms

```csharp
options.String.Transform = StringTransforms.Trim;        // "  Alice  " → "Alice"
options.String.Transform = StringTransforms.Uppercase;  // "alice" → "ALICE"
options.String.Transform = StringTransforms.Trim | StringTransforms.Lowercase; // "  Alice  " → "alice"
```

### Guid format

```csharp
options.Guid.Format = GuidFormat.Hyphenated;    // 00000000-0000-0000-0000-000000000000
options.Guid.Format = GuidFormat.DigitsOnly;    // 00000000000000000000000000000000
options.Guid.Format = GuidFormat.Braces;        // {00000000-0000-0000-0000-000000000000}
options.Guid.Format = GuidFormat.Parentheses;   // (00000000-0000-0000-0000-000000000000)
options.Guid.Case   = GuidCase.Upper;           // uppercase letters
```

### DateTime to string

```csharp
options.DateTime.StringFormat = "O";                    // ISO 8601 round-trip
options.DateTime.TimeZone     = DateTimeZone.ToUtc;     // normalize to UTC before formatting
options.DateTime.Culture      = "pt-BR";                // culture-aware formatting
```

### Numeric rounding and overflow

```csharp
options.Numeric.DecimalToIntRounding = NumericRounding.Nearest;   // Math.Round MidpointRounding.AwayFromZero
options.Numeric.Overflow             = NumericOverflow.Clamp;     // clamp to target type bounds
```

### Bool to string

```csharp
options.Bool.StringFormat = BoolStringFormat.YesOrNo;              // "Yes" / "No"
options.Bool.StringFormat = BoolStringFormat.LowercaseTrueOrFalse; // "true" / "false"
options.Bool.StringFormat = BoolStringFormat.OneOrZero;            // "1" / "0"
options.Bool.StringFormat = BoolStringFormat.Custom;
options.Bool.TrueValue    = "Ativo";
options.Bool.FalseValue   = "Inativo";
```

---

## 🏷️ Per-Property Overrides

Use `[MapConversion]` on any destination property to override global options for that property only. The attribute is read at plan compilation — zero runtime cost.

```csharp
public class UserDto
{
    public string Name { get; set; }

    [MapConversion(GuidFormat = GuidFormat.DigitsOnly, GuidCase = GuidCase.Upper)]
    public string ExternalId { get; set; }

    [MapConversion(DateFormat = "dd/MM/yyyy", DateTimeZone = DateTimeZone.ToLocal)]
    public string CreatedAt { get; set; }

    [MapConversion(NumericRounding = NumericRounding.Nearest)]
    public int Score { get; set; }

    [MapConversion(BoolFormat = BoolStringFormat.YesOrNo)]
    public string IsActive { get; set; }
}
```

---

## 🔒 Strict Mode

Enable strict mode to throw at plan-compile time when a destination property has no matching source property:

```csharp
options.StrictMode = true;
```

Useful during development to catch renaming mismatches early. Disable in production for forward-compatible DTOs.

---

## 🎯 Design Philosophy

Hydrix.Mapper is built around the following principles:

- Compile once, execute indefinitely
- Zero reflection on the hot path
- Performance first, without sacrificing correctness
- Explicit conversion rules baked into compiled expressions
- Thread-safe by design — no locks on the hot path

---

## ❤️ Supporting Hydrix

Hydrix is an open-source project built and maintained with care, transparency, and a long-term vision.

If Hydrix.Mapper helps you build reliable, predictable, and high-performance projection layers, consider supporting the project. Your support helps ensure ongoing maintenance, improvements, documentation, and long-term sustainability.

You can support Hydrix through GitHub Sponsors:

👉 https://github.com/sponsors/marcelo-mattos

Every contribution, whether financial or by sharing feedback and usage experiences, is deeply appreciated.

---

## 📄 License

This project is licensed under the Apache License 2.0.
See the LICENSE and NOTICE files for details.

---

## 👨‍💻 Author

**Marcelo Matos dos Santos**
Software Engineer • Open Source Maintainer.
Engineering clarity. Delivering transformation.
