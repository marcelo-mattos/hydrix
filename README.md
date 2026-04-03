# Hydrix

![NuGet](https://img.shields.io/nuget/v/Hydrix)
![NuGet Downloads](https://img.shields.io/nuget/dt/Hydrix)
![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=marcelo-mattos_hydrix&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=marcelo-mattos_hydrix)

⚡**A high-performance, lightweight, provider-agnostic SQL materializer for .NET.**

Hydrix is a **micro-ORM** built for developers who demand:

- Full control over SQL execution
- Explicit and predictable behavior
- High performance without hidden abstractions
- Efficient hierarchical entity materialization

It intentionally sits between **raw [ADO.NET](https://learn.microsoft.com/pt-br/dotnet/framework/data/adonet/ado-net-overview)** and heavyweight ORMs such as Entity Framework, offering a Dapper-like experience enhanced with:

- Nested entity materialization
- Intelligent metadata caching
- Optimized enum handling
- Zero reflection in the hot path

---

## 🧭 Why Hydrix?

Hydrix is designed for performance-sensitive systems where:

* SQL must remain explicit and visible
* Developers retain full control over execution
* Behavior must be predictable and transparent
* Object graphs must be materialized efficiently from flat JOINs

Hydrix does not attempt to abstract SQL away from you.

---

## ⚠️ What Hydrix is not?

* A LINQ provider
* An automatic SQL generator
* An entity tracking or state management framework

---

## ⚙️ Supported frameworks

* .NET Core 3.1
* .NET 6
* .NET 8
* .NET 10

---

## ✨ Key Features

* Explicit SQL execution (Text and Stored Procedures)
* Strongly typed stored procedure support with `IProcedure<TDataParameter>`
* Entity materialization via standard .NET DataAnnotations
* Nested entity support (flat JOIN → object graph)
* Thread-safe metadata caching
* Process-wide hot cache optimizations for metadata/materialization internals
* Zero reflection in the materialization hot path
* Compiled enum converters (no `Enum.ToObject` per row)
* Optional per-call timeout (`int? timeout`) in execution/query APIs
* Configuration support via `HydrixOptions`
* Dependency Injection integration via `AddHydrix(...)`
* Native SQL `IN` clause expansion
* SQL command logging
* Fully provider-agnostic ([ADO.NET](https://learn.microsoft.com/pt-br/dotnet/framework/data/adonet/ado-net-overview))
* No non-Microsoft dependencies 
* Apache 2.0 licensed

---

## 🆕 What's New in Hydrix 2.1.0

* Configuration and DI integration (`HydrixOptions` and `AddHydrix`)
* Strongly typed procedure execution using `IProcedure<TDataParameter>`
* Optional timeout support across command/query execution APIs
* Internal execution pipeline refactoring (`CommandEngine` and `ParameterEngine`)
* Improved conversion flow (`As<T>`, `Guid`, and provider `DbType` handling)
* Expanded test coverage and validation hardening

---

## ⚡ Performance Design (Hydrix 2.x)

* Hydrix 2.x introduces architectural improvements focused on runtime efficiency:
* Metadata is built once per type and cached
* Property setters are compiled into delegates
* Enum conversions use compiled converters
* No reflection during row materialization
* No `Activator.CreateInstance` in hot paths
* No `Enum.ToObject` in hot paths
* Minimal allocations during nested resolution
* Improved cache topology for faster repeated access
* Lower overhead in conversion and command execution paths

Hydrix is engineered for predictable runtime behavior and low GC pressure.

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

---

### Querying Entities

```csharp
var orders = conn.Query<Order>(
    "SELECT id, total FROM orders WHERE total > @min",
    new { min = 100 },
    timeout: 30
);
```

---

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
});
```

Use this configuration to centralize command timeout and parameter conventions.

---

## 🧱 Defining Entities

### Simple Entity

```csharp
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

Hydrix supports strongly typed procedure parameters through `IProcedure<TDataParameter>`, allowing provider-specific parameter drivers while keeping procedure contracts explicit.

---

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

---

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

```
Executing DbCommand
SELECT * FROM orders WHERE id IN (@ids_0, @ids_1)
Parameters:
  @ids_0 = 'a3f9...' (Guid)
  @ids_1 = 'b4c1...' (Guid)
```

---

## 🧩 Provider Compatibility

Hydrix works with any ADO.NET-compatible provider:

* SQL Server
* PostgreSQL
* MySQL
* Oracle
* DB2
* Others

---

## 🎯 Design Philosophy

Hydrix is built around the following principles:

* Explicit SQL
* Deterministic behavior
* Performance first
* No hidden abstractions
* [ADO.NET](https://learn.microsoft.com/pt-br/dotnet/framework/data/adonet/ado-net-overview) as a solid foundation

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
