---
name: restworld-create-dtos
description: 'Create RESTworld DTOs for a given entity. Use when asked to create DTOs, add DTO types, scaffold DTOs, generate Data Transfer Objects, or create CreateDto/UpdateDto/GetFullDto/GetListDto/QueryDto for an entity. Also handles: creating enums used by DTOs, placing DTOs in the correct .Common project, adding foreign key navigation properties, and asking the user which properties to include in the GetListDto.'
argument-hint: 'Provide the entity class (paste or describe it). Optionally name the target .Common project and the desired namespace.'
---

# RESTworld: Create DTOs for an Entity

## When to Use
- You need to scaffold all five DTO types for a new or existing entity
- You need to add a single DTO type (CreateDto, UpdateDto, GetFullDto, GetListDto, QueryDto) for an entity
- You need to create enums that are referenced by an entity or its DTOs
- You need guidance on which properties belong in which DTO type

## Key Rules (memorize before generating)

1. **Location**: All DTOs and enums go into the C# project whose name ends with `.Common`, in the subfolders `Dtos/` and `Enums/` respectively.
2. **Namespace**: derive from the project name — e.g. `ExampleBlog.Common.Dtos` or `ExampleBlog.Common.Enums`.
3. **Base classes**:

   | DTO type | Base class |
   |---|---|
   | `TCreateDto` | *(none)* |
   | `TUpdateDto` | `ConcurrentDtoBase` |
   | `TQueryDto` | `ChangeTrackingDtoBase` |
   | `TGetListDto` | `ChangeTrackingDtoBase` |
   | `TGetFullDto` | `ChangeTrackingDtoBase` |

4. **Always included in `GetListDto`** (via base class — never ask about these, always present if on the entity): `Id`, `Timestamp`, `CreatedAt`, `CreatedBy`, `LastChangedAt`, `LastChangedBy`.
5. **Foreign keys**: every FK scalar (`long AuthorId`) must be paired with a nullable navigation property whose name matches the stripped suffix (e.g. `AuthorId` → `Author`). Use `[JsonIgnore]` on navigation properties in all DTOs **except** `TQueryDto`. Navigation property type = the target entity's `TGetListDto` if it exists; otherwise fall back to a simpler `<Target>Dto` if one exists in the `Dtos/` folder. Always check existing files before deciding. Never invent a DTO type that does not exist.
6. **`TQueryDto` navigation properties**: do NOT use `[JsonIgnore]` — OData needs them visible.
7. **`TUpdateDto` navigation properties**: do NOT include navigation properties (only scalar FK IDs).
8. **Naming and casing**:
   - DTO class names and all property names are always **PascalCase** and **English**, regardless of the entity's naming convention.
   - If an entity property name is not PascalCase or not English, translate it and add `[Display(Name = "Original Name")]` to document the mapping.
   - Add `[Display(Name = "Multi Word Label")]` to any property whose PascalCase name is composed of multiple words (e.g. `ALongExample` → `[Display(Name = "A Long Example")]`).
9. **Date/time types**: never use `DateTime`. Always use `DateTimeOffset` in DTOs.
10. **Annotations**:
    - `[Display(Name = "Human Name")]` on every FK ID property and every multi-word property.
    - `[Required]` on non-nullable string properties.
    - `[DataType(DataType.MultilineText)]` on long text fields.
    - Non-nullable FK `long` → required field in the form. Nullable `long?` → optional.

## Scripts

All five DTO files are created by dedicated PowerShell scripts located next to this skill:

| Script | Purpose |
|---|---|
| [`New-Enum.ps1`](./scripts/New-Enum.ps1) | Create an enum in `Enums/` |
| [`New-CreateDto.ps1`](./scripts/New-CreateDto.ps1) | `TCreateDto` — no base class |
| [`New-UpdateDto.ps1`](./scripts/New-UpdateDto.ps1) | `TUpdateDto` — `ConcurrentDtoBase` |
| [`New-GetFullDto.ps1`](./scripts/New-GetFullDto.ps1) | `TGetFullDto` — `ChangeTrackingDtoBase` |
| [`New-GetListDto.ps1`](./scripts/New-GetListDto.ps1) | `TGetListDto` — `ChangeTrackingDtoBase` |
| [`New-QueryDto.ps1`](./scripts/New-QueryDto.ps1) | `TQueryDto` — `ChangeTrackingDtoBase` |

**`New-Enum.ps1` parameters:**

| Parameter | Required | Description |
|---|---|---|
| `-EnumName` | ✓ | PascalCase English enum name, e.g. `PostState` |
| `-CommonFolder` | ✓ | Absolute path to the folder containing the `.Common.csproj` file |
| `-Members` | — | `string[]` — one element per member; may include a leading `[Display(...)]` attribute line |
| `-UnderlyingType` | — | Optional underlying type, e.g. `byte`; defaults to `int` |
| `-IsFlags` | — | Switch; adds `[Flags]` attribute when specified |

**DTO script parameters:**

| Parameter | Description |
|---|---|
| `-EntityName` | PascalCase English entity name, e.g. `Post` |
| `-CommonFolder` | Absolute path to the folder containing the `.Common.csproj` file |
| `-Properties` | `string[]` — one element per property, including all attribute lines |

The namespace is derived automatically from the `.csproj` filename found in `-CommonFolder`.  
The scripts create the `Dtos/` subfolder if it does not exist.  
Using directives (`System.ComponentModel.DataAnnotations`, `System.Text.Json.Serialization`, `RESTworld.Common.Dtos`, and the project's own `.Enums` namespace) are added automatically based on what is present in the supplied properties.

## Procedure

### Step 1 — Locate the .Common project

Find the C# project folder in the solution whose name ends with `.Common`.  
The path to that folder is the value to pass as `-CommonFolder`.

All DTO files for an entity go into:
```
<CommonFolder>/Dtos/<EntityName>/
```
Each script creates this subfolder automatically if it does not exist.

### Step 2 — Read the entity

Read the entity class to discover:
- All scalar properties (value types, strings, etc.)
- FK scalar properties (`long XxxId`) and their navigation counterparts
- Any enum-typed properties
- Any collection navigation properties (many-to-many)
- Any `DateTime` properties (must become `DateTimeOffset` in DTOs)
- Non-PascalCase or non-English property names (must be translated and annotated)

### Step 3 — Resolve navigation property target types

For each FK, check the `Dtos/` folder of the `.Common` project for an existing DTO to use as the navigation property type:

1. Prefer `<Target>GetListDto` if it exists.
2. Otherwise use any `<Target>Dto` (simple DTO without a role suffix) if it exists.
3. If neither exists, note that the target DTO must be created first and ask the user how to proceed.

### Step 4 — Ask about GetListDto properties

The `GetListDto` is a compact list view. The following properties are **always included** (via `ChangeTrackingDtoBase`) — do not ask about them:

> `Id`, `Timestamp`, `CreatedAt`, `CreatedBy`, `LastChangedAt`, `LastChangedBy`

For all remaining candidate properties, ask the user which to include using a structured question with checkboxes. Reasonable defaults to pre-select:
- Short string fields like `Name`, `Title`, `Headline`, `Code`
- FK ID fields (so the list can display linked entities)
- Status/state enum fields

If the set of remaining properties is small (≤ 3), you may skip asking and include them all with a note.

### Step 5 — Create enums

For every enum type referenced by the entity that does not yet exist in `<CommonFolder>/Enums/`, run [`New-Enum.ps1`](./scripts/New-Enum.ps1).

- Enum names and member names must be **PascalCase** and **English**.
- If the entity uses non-English or non-PascalCase member names, translate them and add a `[Display(Name = "...")]` attribute element to document the original name.
- Check `<CommonFolder>/Enums/` first — do not recreate an enum that already exists with the same or equivalent members.

```powershell
& "$scriptsDir\New-Enum.ps1" `
    -EnumName  "PostState" `
    -CommonFolder $common `
    -Members  @("Draft", "Published", "Archived")

# With [Display] attributes on members:
& "$scriptsDir\New-Enum.ps1" `
    -EnumName  "OrderStatus" `
    -CommonFolder $common `
    -Members  @(
        "[Display(Name = `"In Progress`")]`n    InProgress",
        "Done",
        "Cancelled"
    )

# Flags enum:
& "$scriptsDir\New-Enum.ps1" `
    -EnumName  "Permission" `
    -CommonFolder $common `
    -Members  @("None = 0", "Read = 1", "Write = 2", "Delete = 4") `
    -IsFlags
```

### Step 6 — Build the property arrays

Before calling the scripts, assemble one `$props*` variable per DTO containing only the properties that belong in that DTO.  
Each element is a single string that may contain newlines for multi-line attribute + declaration pairs.

**Property string format** (each attribute on its own line, declaration last):

```powershell
"[Required]`n    public string Headline { get; set; } = default!;"

"[Display(Name = `"Author`")]`n    public long AuthorId { get; set; }"

"[JsonIgnore]`n    public virtual AuthorGetListDto? Author { get; set; }"
```

**Per-DTO inclusion rules:**

| Property kind | CreateDto | UpdateDto | GetFullDto | GetListDto | QueryDto |
|---|---|---|---|---|---|
| Scalar | ✓ | ✓ | ✓ | if chosen | ✓ |
| FK ID | ✓ | ✓ | ✓ | if chosen | ✓ |
| Navigation (`[JsonIgnore]`) | ✓ | ✗ | ✓ | ✓ | — |
| Navigation (no `[JsonIgnore]`) | ✗ | ✗ | ✗ | ✗ | ✓ |
| `Id` / `Timestamp` / audit | ✗ | via base | via base | via base | via base |

> **UpdateDto**: never include navigation properties.  
> **QueryDto**: navigation properties are the same as for the other DTOs **minus** the `[JsonIgnore]` attribute.

### Step 7 — Run the scripts

```powershell
$entity = "Post"
$common = "C:\Repos\MyApp\MyApp.Common"
$scriptsDir = "<path-to-this-skill>\scripts"

# 1. Enums first (skip any that already exist)
& "$scriptsDir\New-Enum.ps1" -EnumName "PostState" -CommonFolder $common `
    -Members @("Draft", "Published", "Archived")

# 2. Properties shared by CreateDto / UpdateDto (scalars + FK IDs, no nav props)
$sharedProps = @(
    "[Required]`n    public string Headline { get; set; } = default!;",
    "[Display(Name = `"Author`")]`n    public long AuthorId { get; set; }",
    "public PostState State { get; set; }"
)

# Navigation properties with [JsonIgnore] — used in Create/GetFull/GetList
$navPropsWithIgnore = @(
    "[JsonIgnore]`n    public virtual AuthorGetListDto? Author { get; set; }"
)

# Same nav props without [JsonIgnore] — used in Query
$navPropsNoIgnore = @(
    "public virtual AuthorGetListDto? Author { get; set; }"
)

# 3. DTOs
& "$scriptsDir\New-CreateDto.ps1"  -EntityName $entity -CommonFolder $common -Properties ($sharedProps + $navPropsWithIgnore)
& "$scriptsDir\New-UpdateDto.ps1"  -EntityName $entity -CommonFolder $common -Properties $sharedProps
& "$scriptsDir\New-GetFullDto.ps1" -EntityName $entity -CommonFolder $common -Properties ($sharedProps + $navPropsWithIgnore)
& "$scriptsDir\New-GetListDto.ps1" -EntityName $entity -CommonFolder $common -Properties ($listProps  + $navPropsWithIgnore)
& "$scriptsDir\New-QueryDto.ps1"   -EntityName $entity -CommonFolder $common -Properties ($sharedProps + $navPropsNoIgnore)
```

Replace `$listProps` with the subset of `$sharedProps` chosen by the user in Step 4.  
Always run enum scripts **before** DTO scripts so the enum types are available when reviewing the generated DTO files.

### Step 8 — Verify

After running the scripts:
- Confirm all five `.cs` files exist in `<CommonFolder>/Dtos/<EntityName>/`.
- Confirm enums exist in `<CommonFolder>/Enums/`.
- Confirm navigation properties in the `QueryDto` file have **no** `[JsonIgnore]`.
- Confirm navigation properties in all other DTO files **do** have `[JsonIgnore]`.
- Confirm the `UpdateDto` file has **no** navigation properties.
- Confirm no `DateTime` — only `DateTimeOffset`.
- Confirm all class and property names are PascalCase and English.
- Confirm every multi-word property name and every FK ID property has `[Display(Name = "...")]`.

## Reference: DTO comparison table

| Property type | CreateDto | UpdateDto | GetFullDto | GetListDto | QueryDto |
|---|---|---|---|---|---|
| Scalar (string, int, …) | ✓ | ✓ | ✓ | if chosen | ✓ |
| FK ID (`long XxxId`) | ✓ | ✓ | ✓ | if chosen | ✓ |
| Navigation property | ✓ `[JsonIgnore]` | ✗ | ✓ `[JsonIgnore]` | ✓ `[JsonIgnore]` | ✓ (no `[JsonIgnore]`) |
| `Id` / `Timestamp` / audit | ✗ | via base | via base | **always** via base | via base |
| `DateTime` | ✗ — use `DateTimeOffset` | ✗ — use `DateTimeOffset` | ✗ — use `DateTimeOffset` | ✗ — use `DateTimeOffset` | ✗ — use `DateTimeOffset` |

## Reference: dto-types documentation

The full DTO role documentation is in [`doc/dto-types.md`](../../../../doc/dto-types.md).
The complete mapper method reference is in [`doc/mapping-and-versioning.md`](../../../../doc/mapping-and-versioning.md).
