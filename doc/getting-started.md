# Getting Started

This guide walks through the recommended solution layout, shows how to bootstrap RESTworld in ASP.NET Core, and points you to a complete reference implementation.

## Explore the example

The **ExampleBlog** sample demonstrates the full stack, including pipelines, authorization, and the Angular client. Browse it at [`src/Example/ExampleBlog`](../src/Example/ExampleBlog) or online at <https://github.com/wertzui/RESTworld/tree/main/src/Example/ExampleBlog>.

## Recommended solution structure

When creating a new API named `MyApi`, structure your solution into focused projects:

- **MyApi** (ASP.NET Core Web API)
  - References `RESTworld.AspNetCore`, `MyApi.Business`
  - Hosts startup code plus any custom controllers
- **MyApi.Business**
  - References `RESTworld.Business`, `MyApi.Data`
  - Provides mappers and business services
- **MyApi.Data**
  - References `RESTworld.EntityFrameworkCore`, `MyApi.Common`
  - Defines Entity Framework Core entities and migrations
- **MyApi.Common**
  - References `RESTworld.Common`
  - Holds DTOs and enums shared across layers
- **MyApi.Client.Angular** *(optional)*
  - References `RESTworld.Client.AspNetCore`
  - Angular app depends on the `@wertzui/ngx-restworld-client` package
- **MyApi.Aspire.AppHost** *(optional)*
  - Created via the Aspire project template (remove `MyApi.Aspire.ServiceDefaults`)
  - References `MyApi` and `MyApi.Client.Angular`
  - Configures service references for integrated local testing

## Bootstrapping the API

Create minimal startup code and let RESTworld add services and middleware for you:

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddRestWorld();
var services = builder.Services;

// Database
builder.AddDbContextFactoryWithDefaults<TDbContext>();

// Optional database migration during startup
builder.MigrateDatabaseDuringStartup<TDbContext>();

// Default CRUD pipeline with a Mapperly mapper
builder.AddCrudPipeline<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>();
builder.AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, MyEntityMapper>();

// Versioned pipelines (optional) — each version gets its own mapper
builder.AddCrudPipeline<TContext, TEntity, TCreateDtoV1, TQueryDtoV1, TGetListDtoV1, TGetFullDtoV1, TUpdateDtoV1>(new ApiVersion(1, 0), true);
builder.AddCrudMapper<TEntity, TCreateDtoV1, TQueryDtoV1, TGetListDtoV1, TGetFullDtoV1, TUpdateDtoV1, MyEntityMapperV1>();
builder.AddCrudPipeline<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>(new ApiVersion(2, 0));
builder.AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, MyEntityMapper>();

// Pipelines with custom services (optional)
builder.AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();
builder.AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, MyEntityMapper>();

// Read-only pipeline with custom service
builder.AddReadPipelineWithCustomService<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService>();
builder.AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, MyEntityReadMapper>();

// Extra validators (optional)
builder.AddValidator<TValidator, TCreateDto, TUpdateDto, TEntity>();

var app = builder.Build();

app.UseRestWorld();

await app.RunAsync();
```

See [Mapping and Versioning](mapping-and-versioning.md) for how to create Mapperly mappers and for details on the mapper interfaces.

Refer to the [Pipeline Overview](pipeline-overview.md) for details on each pipeline type, and see [Authorization](authorization.md) to plug in access checks.

## Adding the Angular frontend (optional)

If you host the Angular SPA via ASP.NET Core, update the frontends `Program.cs` from the **MyApi.Client.Angular** project:

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddRestWorldWithSpaFrontend();

var app = builder.Build();

app.UseRestWorldWithSpaFrontend();

await app.RunAsync();
```

Continue with [Configuration](configuration.md) to wire RESTworld settings for both API and client projects.
