# Hydrix

![NuGet](https://img.shields.io/nuget/v/Hydrix)
![NuGet Downloads](https://img.shields.io/nuget/dt/Hydrix)
![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=marcelo-mattos_hydrix&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=marcelo-mattos_hydrix)

⚡ **A high-performance, lightweight, provider-agnostic SQL materializer for .NET.**

Hydrix is a **micro-ORM** built for developers who demand:

- Full control over SQL execution
- Explicit and predictable behavior
- High performance without hidden abstractions
- Efficient hierarchical entity materialization

Starting with **Hydrix 3.0.0**, the library is fully centered on **`HydrixDataCore`** as the single runtime entry point. The legacy `Materializer` stack is gone, the execution model is leaner, and the current benchmark suite places Hydrix ahead of Dapper in both flat and nested reads while allocating substantially less memory.

---

## 🧭 Why Hydrix?

Hydrix is designed for performance-sensitive systems where:

- SQL must remain explicit and visible
- Developers retain full control over execution
- Behavior must be predictable and transparent
- Object graphs must be materialized efficiently from flat JOINs

Hydrix does not attempt to abstract SQL away from you.

---

## ⚠️ What Hydrix is not

- A LINQ provider
- An automatic SQL generator
- An entity tracking or state management framework
- A hidden abstraction over ADO.NET

---

## ⚙️ Supported frameworks

- .NET Core 3.1
- .NET 6
- .NET 8
- .NET 10

---

## ✨ Key Features

- `HydrixDataCore` extension-first runtime model
- Explicit SQL execution for text commands and stored procedures
- Strongly typed stored procedure support with `IProcedure<TDataParameter>`
- Entity materialization via DataAnnotations or additive Entity Framework
  model translation
- Entity Framework interoperability through
  `HydrixEntityFramework.RegisterModel(...)`,
  `AddHydrixEntityFrameworkModel<TDbContext>()`, and
  `UseHydrixEntityFrameworkModels()`
- Nested entity support for flat JOIN projections
- Schema-aware metadata and binding caches
- Zero reflection in the materialization hot path
- Optimized scalar conversion pipeline with cached converters
- Opt-in SQL command logging
- Native SQL `IN` clause expansion
- Fully provider-agnostic ADO.NET integration
- No non-Microsoft runtime dependencies
- Apache 2.0 licensed

---

## 🆕 What’s New in Hydrix 3.0.0

- `Materializer` and `IMaterializer` were removed from the public runtime model.
- `HydrixDataCore` is now the only supported way to execute commands and queries.
- Entity Framework interoperability arrived through
  `HydrixEntityFramework.RegisterModel(...)`, with startup/DI automation via
  `AddHydrixEntityFrameworkModel<TDbContext>()` and
  `UseHydrixEntityFrameworkModels()`, allowing Hydrix to read
  `OnModelCreating` metadata and cache it alongside the existing
  attribute-based pipeline.
- `ObjectExtensions` gained faster conversion paths for numeric values,
  `DateTimeOffset`, `TimeSpan`, and broader boolean aliases.
- Converter resolution now uses cache entries keyed by `(sourceType, targetType)`.
- Command and procedure execution hot paths now avoid per-call
  parameter-binding closures and use atomic hot-cache entries for
  stronger concurrency correctness.
- `HydrixOptions` gained `EnableCommandLogging` for explicit logging control.
- Row materializer execution was tightened through direct delegate invocation and
  cached `MethodInfo`.
- Fallback type matching and schema binding were improved to reduce allocations
  and improve throughput in provider edge cases.

---

## ❗ Breaking Changes in 3.0.0

Hydrix 3.0.0 is a **breaking release**.

- `Materializer` was completely removed.
- `IMaterializer` and all Materializer-based execution/query APIs were removed.
- Applications must now use the `HydrixDataCore` extension-based API surface.

If your project is already using the connection extension methods such as `Execute`, `ExecuteScalar`, `Query`, `QueryFirst`, `QuerySingle`, and related overloads, the migration path is straightforward.

---

## 📊 Benchmark Snapshot vs Dapper

The benchmark suite used for Hydrix 3.0.0 compares Hydrix against ADO.NET,
Dapper, and Entity Framework. The release snapshot below highlights the Dapper
baseline:

### Flat

| Scenario | Hydrix | Dapper | Hydrix Gain |
| --- | ---: | ---: | ---: |
| Take 1000 | 986.3 us | 1,062.0 us | 7.1% faster |
| Take 10000 | 10,362.5 us | 12,731.9 us | 18.6% faster |

| Allocation | Hydrix | Dapper | Hydrix Reduction |
| --- | ---: | ---: | ---: |
| Take 1000 | 95.77 KB | 166.40 KB | 42.4% less |
| Take 10000 | 1039.01 KB | 1742.63 KB | 40.4% less |

### Nested

| Scenario | Hydrix | Dapper | Hydrix Gain |
| --- | ---: | ---: | ---: |
| Take 1000 | 1.684 ms | 1.744 ms | 3.4% faster |
| Take 10000 | 17.805 ms | 21.089 ms | 15.6% faster |

| Allocation | Hydrix | Dapper | Hydrix Reduction |
| --- | ---: | ---: | ---: |
| Take 1000 | 135.24 KB | 252.84 KB | 46.5% less |
| Take 10000 | 1430.18 KB | 2602.50 KB | 45.0% less |

Hydrix 3.0.0 ships with a runtime that is not only faster than Dapper in the current suite, but also materially more memory-efficient, especially in nested materialization.

---

## ⚡ Performance Design in 3.0

Hydrix 3.0 continues the performance work started in 2.x and tightens the runtime in a few key places:

- Metadata is built once per type and reused
- Row materializers use direct delegate invocation in hot paths
- Command and procedure parameter binding avoid per-call closure allocations
- Schema binding and matching avoid unnecessary work under contention
- Converter and binder hot caches use atomic entries for low-overhead correctness under concurrency
- Conversion fallbacks rely less on generic `Convert.ChangeType`
- Provider fallback type matching avoids extra boxing when metadata is incomplete
- Limit-based reads stop as early as possible

The result is lower GC pressure, more predictable latency, and stronger nested-materialization throughput.

---

## 📦 Installation

```bash
dotnet add package Hydrix
```

---

## 🚀 Basic Usage

### Executing SQL Commands

```csharp
conn.Execute(
    "INSERT INTO orders (id, total) VALUES (@id, @total)",
    new
    {
        id = Guid.NewGuid(),
        total = 150.75m
    },
    timeout: 30
);
```

### Querying Entities

```csharp
var orders = conn.Query<Order>(
    "SELECT id, total FROM orders WHERE total > @min",
    new { min = 100 },
    timeout: 30
);
```

### Native `IN` Clause Support

```csharp
var orders = conn.Query<Order>(
    "SELECT * FROM orders WHERE id IN (@ids)",
    new
    {
        ids = new[] { id1, id2, id3 }
    }
);
```

Hydrix automatically expands:

```sql
WHERE id IN (@ids_0, @ids_1, @ids_2)
```

Each value is safely parameterized.

---

## 🧩 Configuration & DI

```csharp
using Hydrix.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddHydrix(options =>
{
    options.CommandTimeout = 60;
    options.ParameterPrefix = "@";
    options.EnableCommandLogging = true;
});
```

Use this configuration to centralize timeout, parameter conventions, and logging behavior.

If your application already resolves `DbContext` instances through dependency injection, you can queue the Entity Framework model registration during service setup and apply it once during startup:

```csharp
using Hydrix.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

services.AddHydrix();
services.AddHydrixEntityFrameworkModel<SalesDbContext>();

using var serviceProvider = services.BuildServiceProvider();
serviceProvider.UseHydrixEntityFrameworkModels();
```

---

## 🤝 Entity Framework Interoperability

Hydrix 3.0.0 can work alongside existing Entity Framework models without
replacing the current attribute-based approach. You can register the `DbContext`
or its `Model` manually, or queue the translation through dependency injection
and execute it once during application startup.

```csharp
using Hydrix.DependencyInjection;
using Hydrix.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Manual registration
using var dbContext = new SalesDbContext(...);
HydrixEntityFramework.RegisterModel(dbContext);
HydrixEntityFramework.RegisterModel(dbContext.Model);

// Startup + DI registration
var services = new ServiceCollection();
services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));
services.AddHydrix();
services.AddHydrixEntityFrameworkModel<SalesDbContext>();

using var serviceProvider = services.BuildServiceProvider();
serviceProvider.UseHydrixEntityFrameworkModels();
```

After registration, Hydrix can reuse the table names, schemas, column names,
keys, and supported reference navigations defined in `OnModelCreating`.

Current 3.0.0 scope:

- additive to the existing DataAnnotations-based pipeline
- only CLR types that implement `ITable` are registered
- reference navigations are translated for nested mappings
- collection navigations are intentionally ignored in 3.0.0

---
## 🧱 Defining Entities

### Simple Entity

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("orders", Schema = "pos")]
public class Order :
    DatabaseEntity, ITable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("total")]
    public decimal Total { get; set; }
}
```

### Nested Entities (Flat JOINs)

```csharp
[Table("orders", Schema = "pos")]
public class Order :
    DatabaseEntity, ITable
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public Guid Id { get; set; }

    [ForeignKey("CustomerId")]
    [Column("customerId")]
    public Guid? CustomerId { get; set; }

    [ForeignTable("customer", Schema = "pos", PrimaryKeys = new[] { "Id" }, ForeignKeys = new[] { "CustomerId" })]
    public Customer Customer { get; set; }
}
```

Hydrix only materializes nested entities when related data is present, preventing empty object creation in LEFT JOIN scenarios.

### Stored Procedures

```csharp
[Procedure("sp_create_order", Schema = "pos")]
public class CreateOrder :
    DatabaseEntity, IProcedure<SqlParameter>
{
    [Parameter("p_id", DbType.Guid)]
    public Guid Id { get; set; }

    [Parameter("p_total", DbType.Decimal)]
    public decimal Total { get; set; }
}
```

---

## 📝 SQL Command Logging

Command logging is opt-in and controlled through `HydrixOptions.EnableCommandLogging`.

Example output:

```text
Executing DbCommand
SELECT * FROM orders WHERE id IN (@ids_0, @ids_1)
Parameters:
  @ids_0 = 'a3f9...' (Guid)
  @ids_1 = 'b4c1...' (Guid)
```

---

## 🧩 Provider Compatibility

Hydrix works with any ADO.NET-compatible provider:

- SQL Server
- PostgreSQL
- MySQL
- Oracle
- DB2
- Others

---

## 🎯 Design Philosophy

Hydrix is built around the following principles:

- Explicit SQL
- Deterministic behavior
- Performance first
- No hidden abstractions
- ADO.NET as a solid foundation

---

## ❤️ Supporting Hydrix

Hydrix is an open-source project built and maintained with care, transparency, and a long-term vision.

If Hydrix helps you build reliable, predictable, and high-performance data access layers, consider supporting the project. Your support helps ensure ongoing maintenance, improvements, documentation, and long-term sustainability.

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
