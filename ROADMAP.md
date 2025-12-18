# Hydrix Roadmap

This document outlines the planned evolution of **Hydrix**.
The roadmap reflects the project's core philosophy: **explicit SQL, predictable behavior, and minimal abstractions**.

Features are added deliberately, prioritizing correctness, transparency, and long-term maintainability over convenience or magic behavior.

---

## 🎯 Guiding Principles

- SQL remains explicit and visible at all times
- No implicit query generation or hidden behaviors
- Performance and predictability over convenience
- Provider-agnostic design based on ADO.NET
- Minimal and stable public API

---

## ✅ v1.x — Stabilization & Refinement

Focus: **robustness, performance, and developer experience**  
No breaking changes.

### Planned
- Metadata caching optimizations to further reduce reflection overhead
- Improved diagnostics and debugging helpers for generated SQL
- Additional guardrails and validations for SQL builders
- Minor performance optimizations in entity materialization
- Improved XML documentation and code comments
- Expanded test coverage, including edge cases for nested entities

---

## 🔄 v1.1 — Diagnostics & Observability

Focus: **visibility and troubleshooting**

### Planned
- Optional SQL diagnostics helpers (debug-only)
- Improved logging extensibility
- Better error messages for invalid mappings and configurations
- Safer defaults for edge cases involving null joins and nested entities

---

## 🚀 v2.0 — Extensibility & Advanced Scenarios

Focus: **extensibility without sacrificing explicitness**

### Planned
- Public extension points for custom materialization strategies
- Pluggable conventions for entity mapping
- Optional support for advanced provider-specific features
  (without compromising provider-agnostic core)
- Internal refactoring to support future enhancements

> ⚠️ Note  
> v2.0 may introduce breaking changes and will follow semantic versioning rules strictly.

---

## ❌ Explicitly Out of Scope

The following features are intentionally **not planned**:

- LINQ provider or expression-based query translation
- Automatic SQL generation
- Entity tracking or change detection
- Lazy loading proxies
- Hidden or implicit query execution

Hydrix will remain a **SQL-first** and **developer-controlled** framework.

---

## 🤝 Contributions & Feedback

Community feedback is welcome and helps guide the roadmap.
Feature requests should align with Hydrix’s guiding principles and avoid introducing hidden behaviors or excessive abstraction.

---

## 👨‍💻 Author

**Marcelo Matos dos Santos**

