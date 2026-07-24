---
name: restworld-choose-pipeline
description: 'Choose the correct RESTworld pipeline implementation strategy given a description of what to build. Use when asked which pipeline to use, how to implement a RESTworld endpoint, whether to use ReadPipeline vs CrudPipeline, when to create a custom controller, whether to derive from ReadServiceBase or CrudServiceBase, or how to register a pipeline in Program.cs. Also handles: detecting when entity foreign key relationships require a custom service, choosing versioned vs unversioned registration, and aligning a new API version with a prior version implementation.'
argument-hint: 'Describe what the endpoint needs to do. Optionally provide: C# entity class names (to check FK relations), a target API version (e.g. v2), and any differences from the previous version.'
---

# RESTworld: Choose a Pipeline

## When to Use
- You are implementing a new API endpoint in a RESTworld project
- You need to decide between `ReadPipeline`, `CrudPipeline`, custom services, or custom controllers
- You need the correct `AddXxx` registration call for `Program.cs`
- You are unsure whether to derive from `ReadServiceBase`, `CrudServiceBase`, `DbServiceBase`, `ServiceBase`, `ReadControllerBase`, or `CrudControllerBase`
- You have been given Entity Framework Core entity class names and need to determine whether their relationships require a custom service
- You need to add a new API version and want to align its implementation with an existing lower version

## Pre-Flight Checks

Before walking the flowchart, apply the two rules below. Their outcomes feed directly into the flowchart steps.

### Rule A — Foreign Key Check ("Do you need more than just mapping?")

When the user provides one or more **Entity Framework Core entity class names**, inspect (or ask the user to confirm) whether any of those entities have foreign key navigation properties that reference each other — directly or transitively through intermediate entities.

**How to check:**
1. Look at each entity class for properties typed as another entity class (navigation properties) or for `long`/`int` properties whose name ends in `Id` that correspond to another entity in the set.
2. If any direct or transitive FK relationship exists between the entities involved in this endpoint, answer **YES** to the "Do you need more functionality than just mapping?" questions (Steps 5 and 6 in the flowchart). Reason: the FK navigation data must be loaded and mapped through extra service logic; a plain pipeline cannot handle this automatically.
3. If no FK relationship exists between the endpoint's entities — i.e. the entities are self-contained simple tables — answer **NO** (plain pipeline is sufficient).

> **Examples of FK relationships that require a custom service:**
> - `Post` has `long BlogId` + `Blog? Blog` → `Post` depends on `Blog`
> - `Post` → `Blog` → `Author` (transitive via `Blog.AuthorId`) — even a transitive chain counts
>
> **Examples where a plain pipeline suffices:**
> - `Tag` with only scalar properties — no navigation properties, no FK columns pointing to other entities in scope

---

### Rule B — Versioning

**Default: unversioned.** Do not add any `ApiVersion` arguments to registration calls unless the user explicitly mentions a version number or versioning.

**When the user requests a specific version:**

1. If **version is 1** (or the first version): register normally — no special alignment needed.  
   Use the `(new ApiVersion(1, 0), true)` overload with `isFirstVersion: true`:
   ```csharp
   builder.AddCrudPipeline<...>(new ApiVersion(1, 0), true);
   ```

2. If **version is N > 1**: locate the implementation of version N-1 (ask the user to share it if not visible in context). Then:
   - Start from the N-1 implementation as a baseline.
   - Apply **only the differences the user has described** (new/changed DTO properties, renamed fields, etc.).
   - Keep everything else identical to the N-1 version (same service overrides, same mapper structure, same controller base if any).
   - Register with `new ApiVersion(N, 0)` (no `isFirstVersion` argument):
     ```csharp
     builder.AddCrudPipeline<...>(new ApiVersion(2, 0));
     ```
   - Add a separate mapper registration for the new version's DTOs.

3. **Never silently infer a version.** If the user says "add versioning" without specifying a number, ask which version they want.

---

## Decision Procedure

Walk through the flowchart questions below **in order** and stop at the first matching leaf.

### Step 1 — Does the feature read one resource or a list of resources mapped to entities from a database?

**If NO → go to [Custom Controller Path](#custom-controller-path)**  
**If YES → go to [Database Pipeline Path](#database-pipeline-path)**

---

### Custom Controller Path

#### Step 2 — Does the endpoint return a resource?

**If YES:**  
Create a custom controller. Derive from **`RestControllerBase`**.  
Then continue to [Step 3 — Does the controller need database access?](#step-3--does-the-controller-need-database-access)

**If NO → go to Step 2b**

#### Step 2b — Does the endpoint return HAL?

| Answer | Implementation |
|--------|----------------|
| **YES** | Custom controller, derive from **`HalControllerBase`** |
| **NO**  | Custom controller, derive from **`Controller`** (plain ASP.NET Core) |

#### Step 3 — Does the controller need database access?

| Answer | Service base class |
|--------|--------------------|
| **NO**  | Derive your own service from **`ServiceBase`** |
| **YES** | Derive your own service from **`DbServiceBase`** |

---

### Database Pipeline Path

#### Step 4 — Do you need to modify resources (create / update / delete)?

**If NO → go to [Read-Only Path](#read-only-path)**  
**If YES → go to [CRUD Path](#crud-path)**

---

### Read-Only Path

#### Step 5 — Do you need more functionality than just mapping?

| Answer | Implementation | Registration |
|--------|----------------|--------------|
| **NO**  | **`ReadPipeline`** — no custom code needed | `builder.AddReadPipeline<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto>();` + `builder.AddReadMapper<...>();` |
| **YES → Step 5b** | | |

#### Step 5b — Do you need more than the read endpoints (extra routes)?

| Answer | Implementation | Registration |
|--------|----------------|--------------|
| **NO**  | **`ReadPipelineWithCustomService`** — derive service from **`ReadServiceBase`**, override `On...` methods | `builder.AddReadPipelineWithCustomService<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService>();` + `builder.AddReadMapper<...>();` |
| **YES** | **Custom controller** — derive controller from **`ReadControllerBase`**, derive service from **`ReadServiceBase`**, override `On...` methods, add methods for extra endpoints | No pipeline registration; wire up manually |

---

### CRUD Path

#### Step 6 — Do you need more functionality than just mapping?

| Answer | Implementation | Registration |
|--------|----------------|--------------|
| **NO**  | **`CrudPipeline`** — no custom code needed | `builder.AddCrudPipeline<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>();` + `builder.AddCrudMapper<...>();` |
| **YES → Step 6b** | | |

#### Step 6b — Do you need more than the CRUD endpoints (extra routes)?

| Answer | Implementation | Registration |
|--------|----------------|--------------|
| **NO**  | **`CrudPipelineWithCustomService`** — derive service from **`CrudServiceBase`**, override `On...` methods | `builder.AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();` + `builder.AddCrudMapper<...>();` |
| **YES** | **Custom controller** — derive controller from **`CrudControllerBase`**, derive service from **`CrudServiceBase`**, override `On...` methods, add methods for extra endpoints | No pipeline registration; wire up manually |

---

## Quick Reference Table

| Scenario | Pipeline / Base | Registration method |
|----------|----------------|---------------------|
| Read-only, no custom logic | `ReadPipeline` | `AddReadPipeline` |
| Read-only + custom service logic, no extra routes | `ReadPipelineWithCustomService` | `AddReadPipelineWithCustomService` |
| Read-only + extra routes | Custom controller (`ReadControllerBase`) + custom service (`ReadServiceBase`) | Manual |
| Full CRUD, no custom logic | `CrudPipeline` | `AddCrudPipeline` |
| Full CRUD + custom service logic, no extra routes | `CrudPipelineWithCustomService` | `AddCrudPipelineWithCustomService` |
| Full CRUD + extra routes | Custom controller (`CrudControllerBase`) + custom service (`CrudServiceBase`) | Manual |
| Non-DB resource endpoint | Custom controller (`RestControllerBase`) + service (`ServiceBase` or `DbServiceBase`) | Manual |
| HAL response, no resource | Custom controller (`HalControllerBase`) | Manual |
| Plain HTTP, no HAL | Custom controller (`Controller`) | Manual |

---

## Output

After walking through the decision procedure, produce:

1. **Recommended pipeline / base classes** — name the exact classes to derive from.
2. **Registration snippet** — the `builder.AddXxx<...>()` calls to put in `Program.cs`, if a pipeline registration exists.
3. **Brief rationale** — one sentence explaining why this choice fits the described requirement.
4. **Next steps** — point to the relevant doc (`getting-started.md`, `mapping-and-versioning.md`, `authorization.md`) for implementation details.

---

## Reference

- [Choosing a Pipeline](../../../doc/choosing_a_pipeline.md) — source flowchart
- [Pipeline Overview](../../../doc/pipeline-overview.md) — request flow explanation
- [Getting Started](../../../doc/getting-started.md) — registration code examples
- [Mapping and Versioning](../../../doc/mapping-and-versioning.md) — mapper interfaces
