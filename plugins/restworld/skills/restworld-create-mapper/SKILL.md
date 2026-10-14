---
name: restworld-create-mapper
description: 'Create and register a RESTworld mapper between an entity and its DTOs. Use when asked to create a mapper, map entity to DTO, write a Mapperly mapper, derive from CrudMapperlyMapperBase or ReadMapperlyMapperBase, implement ICrudMapper / IReadMapper, maintain or migrate legacy AutoMapper code (AddCrudAutoMapper / AddReadAutoMapper) to Mapperly, register a mapper with AddCrudMapper / AddReadMapper / AddCreateMapper / AddUpdateMapper, ignore infrastructure properties (Id, Timestamp, CreatedAt, LastChangedBy), map navigation properties or foreign keys, fix Mapperly diagnostics about unmapped members, or create a separate mapper per API version. Covers every abstract method that must be implemented and which DTO each one maps.'
argument-hint: 'Provide the entity and its DTO types (CreateDto, QueryDto, GetListDto, GetFullDto, UpdateDto) or ask for a read-only mapper. Optionally state the API version.'
---

# RESTworld: Create a Mapper

## When to Use
- Every pipeline (`AddCrudPipeline`, `AddReadPipeline`, `...WithCustomService`) needs exactly one registered mapper for its entity/DTO combination
- You are writing a custom service or controller that needs `IReadMapper<...>` / `ICrudMapper<...>`
- You are adding a new API version with different DTOs (one mapper per DTO set)

Prerequisites: entity ([restworld-create-entity-and-dbcontext](../restworld-create-entity-and-dbcontext/SKILL.md)) and DTOs ([restworld-create-dtos](../restworld-create-dtos/SKILL.md)).

## Choosing the Mapper Type

| Situation | Use | Registration (on `IHostApplicationBuilder`/`IServiceCollection`) |
|---|---|---|
| CRUD pipeline (**recommended**) | Mapperly: `CrudMapperlyMapperBase<TEntity,TCreateDto,TQueryDto,TGetListDto,TGetFullDto,TUpdateDto>` | `AddCrudMapper<..., TMapper>()` |
| Read-only pipeline (**recommended**) | Mapperly: `ReadMapperlyMapperBase<TEntity,TQueryDto,TGetListDto,TGetFullDto>` | `AddReadMapper<..., TMapper>()` |
| Existing code only (**legacy**, never for new code) | AutoMapper profiles | `AddCrudAutoMapper<TEntity,TCreateDto,TQueryDto,TGetListDto,TGetFullDto,TUpdateDto>()` / `AddReadAutoMapper<TEntity,TQueryDto,TGetListDto,TGetFullDto>()` |
| Full manual control | implement `ICrudMapper` / `IReadMapper` yourself | `AddCrudMapper` / `AddReadMapper`; also `AddCreateMapper<TEntity,TCreateDto,TMapper>()` / `AddUpdateMapper<TEntity,TUpdateDto,TMapper>()` |

Mapperly classes are in `RESTworld.Business.Mapping.Mapperly`, interfaces in `RESTworld.Business.Mapping`. Mappers are registered as **singletons** (stateless, no injected scoped services). Place them in `MyApi.Business/Mapping/`.

## Interface Methods to Implement (Mapperly: all `partial`)

| Method | Maps | Notes |
|---|---|---|
| `MapCreateToEntity(TCreateDto)` | CreateDto -> new entity | Ignore `Id`, `Timestamp`, `CreatedAt/By`, `LastChangedAt/By`, navigations |
| `MapUpdateToEntity(TUpdateDto, TEntity)` | UpdateDto -> existing entity (`void`) | Same ignores; the `Timestamp` is already consumed by the concurrency check |
| `MapEntityToQueryQueryable(IQueryable<TEntity>)` | entity -> `TQueryDto` projection | Stage 1, before OData; include navigations needed for `$filter`/`$orderby` |
| `MapQueryToListQueryable(IQueryable<TQueryDto>)` | `TQueryDto` -> `TGetListDto` projection | Stage 2 |
| `MapQueryToFullQueryable(IQueryable<TQueryDto>)` | `TQueryDto` -> `TGetFullDto` projection | Stage 2 |
| `MapEntityToFullQueryable(IQueryable<TEntity>)` | entity -> `TGetFullDto` projection | Single `GET {id}` |
| `MapQueryToList(TQueryDto)` | single `TQueryDto` -> `TGetListDto` | `public abstract` |
| `MapQueryToFull(TQueryDto)` | single `TQueryDto` -> `TGetFullDto` | `public abstract` |
| `MapEntityToFull(TEntity)` | entity -> `TGetFullDto` | **`protected abstract`** (so declare it `protected override partial`) |
| `MapEntityToFullExpression()` | returns `Expression<Func<TEntity,TGetFullDto>>` | Used to build `MemberMappingNames` (DB column -> DTO property in error messages) |

The `IReadMapper` interface itself only contains the four `...Queryable` methods plus `MemberMappingNames`; the single-object methods (`MapQueryToList`, `MapQueryToFull`, `MapEntityToFull`) and `MapEntityToFullExpression` are part of the Mapperly base class. `MapEntityToQuery` does not exist (it is `MapEntityToQueryQueryable`). Background on the two stages: [DTO Types and Roles](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md).

## Example: Simple Entity (ExampleBlog `BlogMapper`)
```csharp
using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using RESTworld.Business.Mapping.Mapperly;
using Riok.Mapperly.Abstractions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExampleBlog.Business.Mapping;

[Mapper]
public partial class BlogMapper : CrudMapperlyMapperBase<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>
{
    // Source = every CreateDto member must be consumed; unmapped entity members (Id, auditing) are fine.
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public override partial Blog MapCreateToEntity(BlogCreateDto createDto);

    // Target = every member of the DTO must be filled; entity-only members (FKs, navigations) are fine.
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    protected override partial BlogGetFullDto MapEntityToFull(Blog entity);

    public override partial Expression<Func<Blog, BlogGetFullDto>> MapEntityToFullExpression();

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetFullDto MapQueryToFull(BlogQueryDto entity);
    public override partial IQueryable<BlogGetFullDto> MapQueryToFullQueryable(IQueryable<BlogQueryDto> entities);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetListDto MapQueryToList(BlogQueryDto queryDto);
    public override partial IQueryable<BlogGetListDto> MapQueryToListQueryable(IQueryable<BlogQueryDto> queryDtos);

    public override partial IQueryable<BlogQueryDto> MapEntityToQueryQueryable(IQueryable<Blog> entities);
    public override partial IQueryable<BlogGetFullDto> MapEntityToFullQueryable(IQueryable<Blog> entities);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public override partial void MapUpdateToEntity(BlogUpdateDto updateDto, Blog entity);
}
```
Register: `rwBuilder.AddCrudMapper<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto, BlogMapper>();`

## Navigation Properties and Foreign Keys (ExampleBlog `PostMapper`)
Rules of thumb:
- Entity -> DTO: FK (`AuthorId`) maps by name; entity navigation (`Post.Author`) is ignored with `[MapperIgnoreSource(nameof(Post.Author))]` for the non-queryable methods when the DTO's navigation is `[JsonIgnore]`/not loaded.
- `TQueryDto` may contain navigation DTOs (`PostQueryDto.Author`, `.Blog`) so OData can filter across relations. Mapperly needs a **private helper** for each nested type to map it, e.g. `private partial AuthorDto MapToAuthorDto(Author source);` (add `[MapperIgnoreSource(nameof(Author.Posts))]` to break cycles).
- DTO -> entity: ignore navigations and infrastructure members with `[MapperIgnoreTarget(nameof(Post.Author))]`, `Blog`, `Id`, `CreatedAt`, `CreatedBy`, `LastChangedAt`, `LastChangedBy`, `Timestamp`. DTO members that are not on the entity (non-DB fields such as `Image`) get `[MapperIgnoreSource(nameof(PostCreateDto.Image))]`.
```csharp
[Mapper]
public partial class PostMapper : CrudMapperlyMapperBase<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    [MapperIgnoreSource(nameof(PostCreateDto.Attachement))]
    [MapperIgnoreSource(nameof(PostCreateDto.Image))]
    [MapperIgnoreSource(nameof(PostCreateDto.Author))]
    [MapperIgnoreSource(nameof(PostCreateDto.Blog))]
    [MapperIgnoreTarget(nameof(Post.Author))]
    [MapperIgnoreTarget(nameof(Post.Blog))]
    [MapperIgnoreTarget(nameof(Post.Id))]
    [MapperIgnoreTarget(nameof(Post.CreatedAt))]
    [MapperIgnoreTarget(nameof(Post.CreatedBy))]
    [MapperIgnoreTarget(nameof(Post.LastChangedAt))]
    [MapperIgnoreTarget(nameof(Post.LastChangedBy))]
    [MapperIgnoreTarget(nameof(Post.Timestamp))]
    public override partial Post MapCreateToEntity(PostCreateDto createDto);

    [MapperIgnoreSource(nameof(Author.Posts))]
    private partial AuthorDto MapToAuthorDto(Author source);   // enables PostQueryDto.Author

    [MapperIgnoreSource(nameof(Blog.Posts))]
    private partial BlogGetFullDto MapToBlogDto(Blog source);  // enables PostQueryDto.Blog

    // ... remaining partial overrides as in BlogMapper
}
```
Read the full file at `src/Example/ExampleBlog/ExampleBlog.Business/Mapping/PostMapper.cs` for all methods. If a mapper cannot express something (e.g. data computed from other sources), map the DB part here and fill the rest in a custom service (see [restworld-custom-service](../restworld-custom-service/SKILL.md)).

## Read-Only Mapper
Derive from `ReadMapperlyMapperBase<TEntity,TQueryDto,TGetListDto,TGetFullDto>` and implement only the read methods above (no create/update). Register with `rwBuilder.AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, TMapper>();`.

## Versioned Mappers
One mapper class **per DTO set**; the pipeline registrations differ only by `ApiVersion`:
```csharp
rwBuilder.AddCrudPipeline<BlogDatabase, Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>(new ApiVersion(1, 0), true);
rwBuilder.AddCrudMapper<Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorMapperV1>();
rwBuilder.AddCrudPipeline<BlogDatabase, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto, AuthorDto>(new ApiVersion(2, 0));
rwBuilder.AddCrudMapper<Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto, AuthorDto, AuthorMapper>();
```
(`AuthorMapperV1` / `AuthorMapper` are names you would create; ExampleBlog intentionally still registers AutoMapper for Author to show the migration path; new code should write Mapperly mappers.) Version-specific conversions (e.g. V1 `Name` = `FirstName + " " + LastName`) belong in the V1 mapper. Full procedure: [restworld-add-api-version](../restworld-add-api-version/SKILL.md).

## Legacy: AutoMapper (existing code only) and migration to Mapperly
```csharp
services.AddAutoMapper(AutoMapperConfiguration.ConfigureAutomapper);   // your profile config (IMapperConfigurationExpression)
rwBuilder.AddCrudAutoMapper<Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>();
rwBuilder.AddReadAutoMapper<Author, AuthorStatisticsListDto, AuthorStatisticsListDto, AuthorStatisticsFullDto>();
```
`AddRestWorld()` already registers AutoMapper with expression mapping. Each entity/DTO pair needs `config.CreateMap<TEntity, TDto>()` (and `.ReverseMap()` or separate maps for create/update). Never use this for new pipelines.

**Migrating a pipeline to Mapperly:** write a Mapperly mapper for the pipeline (above), swap `AddCrudAutoMapper`/`AddReadAutoMapper` for `AddCrudMapper`/`AddReadMapper` with the new mapper, move `ForMember` rules into `[MapProperty]` or private helpers, delete the matching `CreateMap` calls, and remove `AutoMapperConfiguration`/`AddAutoMapper` once no pipeline needs it. One pipeline can be migrated at a time.

## Verification Checklist
- [ ] Class is `[Mapper] public partial class` and derives from the correct `...MapperlyMapperBase`
- [ ] All abstract methods are declared `partial` with the right visibility (`MapEntityToFull` is `protected`)
- [ ] Infrastructure properties are ignored on create/update; navigations ignored or mapped via private helpers
- [ ] No Mapperly warnings/errors on `dotnet build` (unmapped members, RMG0xx)
- [ ] Mapper registered with matching generic arguments (same TEntity/DTOs as the pipeline); one registration per pipeline/version
- [ ] Mapper has no scoped dependencies (it is a singleton)

## Reference
- [Mapping and Versioning](https://github.com/wertzui/RESTworld/blob/main/doc/mapping-and-versioning.md)
- [DTO Types and Roles](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md)
- [Getting Started](https://github.com/wertzui/RESTworld/blob/main/doc/getting-started.md)
