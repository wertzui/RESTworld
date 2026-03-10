# Mapping and Versioning

RESTworld uses a mapper abstraction to convert between entities and DTOs. You can use **Mapperly** (recommended), **AutoMapper** (deprecated), or any custom implementation. Each pipeline must have a matching mapper registered in the dependency injection container.

> Before diving into mappers, familiarise yourself with the different DTO types and their roles in the pipeline — see [DTO Types and Roles](dto-types.md).

## Mapper registration

Every pipeline needs a mapper. RESTworld provides extension methods on both `IServiceCollection` and `IHostApplicationBuilder` to register them.

### Using Mapperly (recommended)

For compile-time source-generated mapping, derive from `CrudMapperlyMapperBase` (or `ReadMapperlyMapperBase` for read-only scenarios) and annotate the class with Mapperly's `[Mapper]` attribute:

```csharp
using RESTworld.Business.Mapping.Mapperly;
using Riok.Mapperly.Abstractions;

namespace MyApi.Business.Mapping;

[Mapper]
public partial class BlogMapper : CrudMapperlyMapperBase<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>
{
    // When creating an entry, everything on ChangeTrackingEntityBase should be ignored.
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public override partial Blog MapCreateToEntity(BlogCreateDto createDto);

    // Foreign Keys should be ignored
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetFullDto MapEntityToFull(Blog entity);

    public override partial Expression<Func<Blog, BlogGetFullDto>> MapEntityToFullExpression();

    // Foreign Keys should be ignored
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetFullDto MapQueryToFull(BlogQueryDto entity);

    public override partial IQueryable<BlogGetFullDto> MapQueryToFullQueryable(IQueryable<BlogQueryDto> entities);

    // Foreign Keys should be ignored
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public override partial BlogGetListDto MapQueryToList(BlogQueryDto queryDto);

    public override partial IQueryable<BlogGetListDto> MapQueryToListQueryable(IQueryable<BlogQueryDto> queryDtos);

    public override partial IQueryable<BlogQueryDto> MapEntityToQuery(IQueryable<Blog> entities);

    // When updating an entry, everything on ChangeTrackingEntityBase except the Id should be ignored.
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public override partial void MapUpdateToEntity(BlogUpdateDto updateDto, Blog entity);
}
```

Register the mapper with the matching `AddCrudMapper` or `AddReadMapper` extension:

```csharp
builder.AddCrudMapper<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto, BlogMapper>();
```

### Using AutoMapper (deprecated)

> **Deprecated.** AutoMapper support is kept for backwards compatibility. New projects should use Mapperly instead.

If your entity-to-DTO mapping is a straightforward bidirectional mapping that AutoMapper can handle, you can use the built-in convenience methods. They create an AutoMapper-backed mapper for you automatically — you only need to configure AutoMapper profiles.

```csharp
// In Program.cs
services.AddAutoMapper(AutoMapperConfiguration.ConfigureAutomapper);

// Register an AutoMapper-backed mapper for a CRUD pipeline
builder.AddCrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Register an AutoMapper-backed mapper for a read-only pipeline
builder.AddReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>();
```

You still need an `AutoMapperConfiguration` class in your `MyApi.Business` that configures the profiles:

```csharp
using AutoMapper;
using MyApi.Common.Dtos;
using MyApi.Common.Enums;
using MyApi.Data.Models;

namespace MyApi.Business;

public static class AutoMapperConfiguration
{
    public static void ConfigureAutomapper(IMapperConfigurationExpression config)
    {
        // Simple bidirectional mapping
        config
            .CreateMap<TEntity, TDto>()
            .ReverseMap();

        // Add mappings for each version as needed
        config
            .CreateMap<MyEntity, MyDto>()
            .ReverseMap();

        config
            .CreateMap<MyEntity, MyDtoV1>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ReverseMap()
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[0]))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[1]));

        // Add more mappings as your domain evolves
    }
}
```

### Custom mappers

You can implement the mapper interfaces directly for full control:

| Interface | Use case |
|-----------|----------|
| `ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>` | Full CRUD pipelines |
| `IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>` | Read-only pipelines |
| `ICreateMapper<TEntity, TCreateDto>` | Create mapping only |
| `IUpdateMapper<TEntity, TUpdateDto>` | Update mapping only |

Register them with the corresponding extension method:

```csharp
services.AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, MyCustomMapper>();
services.AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, MyCustomReadMapper>();
```

## Mapper method reference

The table below describes every method in `IReadMapper` (and therefore `ICrudMapper`). For background on the two-stage query design and why `TQueryDto` exists, see [DTO Types and Roles — TQueryDto](dto-types.md#tquerydto--the-odata-intermediary).

| Method | Stage | Purpose |
|---|---|---|
| `MapEntityToQuery(IQueryable<TEntity>)` | 1 — before OData | Project entities to `TQueryDto`. Include navigation properties so OData can generate the correct SQL joins for `$filter` and `$orderby` across related data. |
| `MapQueryToList(TQueryDto)` | 2a — after OData | Map one `TQueryDto` to `TGetListDto`. Drop navigation properties and any fields not needed in list views. |
| `MapQueryToListQueryable(IQueryable<TQueryDto>)` | 2a — after OData | Queryable projection of `MapQueryToList`. Auto-generated by Mapperly; manually implement or delegate if using AutoMapper. |
| `MapQueryToFull(TQueryDto)` | 2b — after OData | Map one `TQueryDto` to `TGetFullDto`. Navigation properties that are `[JsonIgnore]` on `TGetFullDto` should be excluded. |
| `MapQueryToFullQueryable(IQueryable<TQueryDto>)` | 2b — after OData | Queryable projection of `MapQueryToFull`. Auto-generated by Mapperly. |
| `MapEntityToFull(TEntity)` | Direct (no OData) | Map a single entity to `TGetFullDto`. Used by the non-queryable `Get` endpoint that fetches a record by ID. Navigation properties and infrastructure fields should be excluded. |
| `MapEntityToFullExpression()` | Metadata | Returns an `Expression<Func<TEntity, TGetFullDto>>` used to build `MemberMappingNames`, which translates database column names in exceptions to DTO property names. Auto-generated by Mapperly. |
| `MapCreateToEntity(TCreateDto)` | Write | Map a create DTO to a new entity. Ignore all infrastructure properties (`Id`, `CreatedAt`, `CreatedBy`, `LastChangedAt`, `LastChangedBy`, `Timestamp`) on the entity side. |
| `MapUpdateToEntity(TUpdateDto, TEntity)` | Write | Apply an update DTO onto an existing entity. Ignore all infrastructure properties except `Id` on the entity side. The `Timestamp` is consumed by the concurrency check before this method runs. |

## Versioning strategy

RESTworld parses API versions from media-type headers and echoes them back so clients can adapt. Configure defaults in [Configuration](configuration.md) and align your DTO mappings accordingly.

- **Preferred approach:** keep all version-specific logic in the mapper to avoid branching inside services. Create two Read- or CRUD pipelines that differ only in the API version and the DTO, each with its own mapper registration.
- **Alternative:** create version-specific services and pipelines when behavior diverges beyond simple mapping. Use the extension methods shown in [Getting Started](getting-started.md) and consult [Choosing a Pipeline](choosing_a_pipeline.md).

```csharp
// Versioned pipelines with separate mappers
builder.AddCrudPipeline<TContext, Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1>(new ApiVersion(1, 0), true);
builder.AddCrudMapper<Author, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorDtoV1, AuthorMapperV1>();

builder.AddCrudPipeline<TContext, Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto>(new ApiVersion(2, 0));
builder.AddCrudMapper<Author, AuthorDto, AuthorDto, AuthorDto, AuthorDto, AuthorMapper>();
```

### Accept header format

Clients should request specific versions via the `Accept` header, for example: `application/hal+json; v=42`. RESTworld replies with the negotiated version in the `Content-Type` header and advertises supported/deprecated versions through `api-supported-versions` and `api-deprecated-versions` response headers.

### Handling deprecated versions

Monitor the `api-deprecated-versions` header in your client implementations and surface warnings proactively. Plan for version transitions by maintaining parallel DTOs and mappings until consumers migrate.

Continue with [Authorization](authorization.md) to secure endpoints or review [Angular Client Development](client-development-angular.md) and [.NET Client Development](client-development-dotnet.md) for version-aware client patterns.
