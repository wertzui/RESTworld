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
  - Provides AutoMapper profiles and business services
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

var rwBuilder = builder.AddRestWorld();
var services = rwBuilder.Services;

// AutoMapper
services.AddAutoMapper(AutoMapperConfiguration.ConfigureAutomapper);

// Database
services.AddDbContextFactoryWithDefaults<TDbContext>(builder.Configuration);

// Optional database migration during startup
services.MigrateDatabaseDuringStartup<TDbContext>();

// Default pipeline
services.AddCrudPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Versioned pipelines (optional)
services.AddCrudPipeline<TContext, TEntity, TCreateDtoV1, TGetListDtoV1, TGetFullDtoV1, TUpdateDtoV1>(new ApiVersion(1, 0), true);
services.AddCrudPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(new ApiVersion(2, 0));

// Pipelines with custom services (optional)
services.AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();

// Extra validators (optional)
services.AddValidator<TValidator, TCreateDto, TUpdateDto, TEntity>();

var app = rwBuilder.Build();

app.UseRestWorld();

await app.RunAsync();
```

Refer to the [Pipeline Overview](pipeline-overview.md) for details on each pipeline type, and see [Authorization](authorization.md) to plug in access checks.

## Adding the Angular frontend (optional)

If you host the Angular SPA via ASP.NET Core, update the frontends `Program.cs` from the **MyApi.Client.Angular** project:

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var rwBuilder = builder.AddRestWorldWithSpaFrontend();

var app = rwBuilder.Build();

app.UseRestWorldWithSpaFrontend();

await app.RunAsync();
```

Continue with [Configuration](configuration.md) to wire RESTworld settings for both API and client projects.
