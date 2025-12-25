# Hydrix

**A lightweight, explicit, and provider-agnostic SQL materialization framework for .NET.**

Hydrix is a **micro-ORM** designed for developers who require **full control over SQL execution**, **predictable behavior**, and **efficient entity materialization**, without introducing hidden abstractions or proprietary query languages.

The framework intentionally sits between **raw [ADO.NET](http://ADO.NET)** and heavyweight ORMs such as Entity Framework, offering a Dapper-like experience enhanced with **hierarchical entity materialization**, **metadata caching**, and **native support for nested entities**.

> ✅ The **Hydrix** package ID prefix is officially reserved on NuGet.org.

![NuGet](https://img.shields.io/nuget/v/Hydrix)
![NuGet Downloads](https://img.shields.io/nuget/dt/Hydrix)
![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)

---

## 🧭 Why Hydrix?

* You want full control over your SQL
* You work with complex or performance-critical queries
* You prefer explicit behavior over hidden abstractions
* You need a lightweight alternative to full ORMs

> Hydrix is designed for developers who want **full control** over their SQL while keeping object hydration explicit, safe, and predictable.

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
* Automatic materialization of entities (`ISqlEntity`)
* Support for nested entities (flat JOINs → object graphs)
* Thread-safe metadata caching with optimized reflection
* Native support for SQL `IN` clauses with safe parameter expansion
* SQL command logging similar to Entity Framework
* Fully provider-agnostic ([ADO.NET](http://ADO.NET) based)
* Compatible with **.NET Core 3.1 and later**
* Zero external dependencies
* Distributed under the Apache-2.0 License

> ⚠️ Note
> SQL builders in Hydrix are stateful by design and should not be reused across multiple queries.

---

## 📦 Installation

```bash
dotnet add package Hydrix
```


---

## 🚀 Basic Usage

### Executing SQL Commands

```csharp
hydrix.ExecuteNonQuery(
    "INSERT INTO orders (id, total) VALUES (@id, @total)",
    new
    {
        id = Guid.NewGuid(),
        total = 150.75m
    }
);
```


---

### Querying Entities

```csharp
var orders = hydrix.Query<Order>(
    "SELECT id, total FROM orders WHERE total > @min",
    new { min = 100 }
);
```


---

### Native `IN` Clause Support

```csharp
var orders = hydrix.Query<Order>(
    "SELECT * FROM orders WHERE id IN (@ids)",
    new
    {
        ids = new[] { id1, id2, id3 }
    }
);
```

The framework automatically expands the query into:

```sql
WHERE id IN (@ids_0, @ids_1, @ids_2)
```

Each value is bound as an individual parameter, ensuring safety and compatibility across providers.


---

## 🧱 Defining Entities

### Simple Entity

```csharp
[SqlEntity]
public class Order : ISqlEntity
{
    [SqlField]
    public Guid Id { get; set; }

    [SqlField]
    public decimal Total { get; set; }
}
```


---

### Nested Entities (Flat JOINs)

```csharp
[SqlEntity]
public class Order : ISqlEntity
{
    [SqlField]
    public Guid Id { get; set; }

    [SqlEntity(PrimaryKey = "Id")]
    public Customer Customer { get; set; }
}
```

The materializer automatically constructs the object graph when the related data is present, preventing the creation of empty nested objects when LEFT JOINs return null values.


---

## 🔄 Transactions

```csharp
hydrix.OpenConnection();
hydrix.BeginTransaction(IsolationLevel.ReadCommitted);

try
{
    hydrix.ExecuteNonQuery(...);
    hydrix.ExecuteNonQuery(...);

    hydrix.CommitTransaction();
}
catch
{
    hydrix.RollbackTransaction();
    throw;
}
```


---

## 📝 SQL Command Logging

```
--------------------------------------------------
Executing DbCommand

SELECT * FROM orders WHERE id IN (@ids_0, @ids_1)

Parameters:
  @ids_0 = 'a3f9...' (Guid)
  @ids_1 = 'b4c1...' (Guid)
--------------------------------------------------
```


---

## 🎯 Design Philosophy

Hydrix is built around the following principles:

* SQL should remain explicit and visible
* Developers must retain full control over execution
* No hidden behaviors or implicit query generation
* Performance, predictability, and transparency over convenience
* [ADO.NET](http://ADO.NET) as a solid and proven foundation


---

## 🧩 Provider Compatibility

* Microsoft SQL Server
* PostgreSQL
* MySQL
* Oracle
* Any ADO.NET-compatible data provider


---

## ❤️ Supporting Hydrix

Hydrix is an open-source project built and maintained with care, transparency, and a long-term vision.

If Hydrix helps you build reliable, predictable, and high-performance data access layers, consider supporting the project. Your support helps ensure ongoing maintenance, improvements, documentation, and long-term sustainability.

You can support Hydrix through GitHub Sponsors:

👉 https://github.com/sponsors/marcelo-matos

Every contribution, whether financial or by sharing feedback and usage experiences, is deeply appreciated.


---

## 📄 License

This project is licensed under the Apache License 2.0. See the LICENSE and NOTICE files for details.


---

## 👨‍💻 Author

**Marcelo Matos dos Santos**  
Software Engineer • Open Source Maintainer.
