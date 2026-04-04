---
name: xml-documentation-completeness
description: All C# members must have complete XML documentation including private members and subclasses
type: feedback
---

All methods, enums, properties, and fields — including private ones and subclass members — must have complete XML documentation:

- `<summary>` — always present, well-descriptive
- `<param name="...">` — for every parameter
- `<returns>` — whenever return type is not void
- `<typeparam name="...">` — for every generic type parameter
- `<remarks>` — when there is context worth documenting (threading, caching, performance, edge cases)

**Why:** User has an explicit documentation standard for the Hydrix library. Missing docs on private/internal members are caught in review.

**How to apply:** Whenever writing or editing any C# file in this project, ensure every member has full XML docs before finishing. This applies equally to private methods, internal fields, nested types, and enum values.
