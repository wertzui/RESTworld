---
name: restworld-bootstrap-solution
description: 'Create a new RESTworld solution from scratch. Use when asked to start a new RESTworld API, bootstrap or scaffold a solution, set up the project layout (Api, Business, Common, Data), choose RESTworld NuGet packages (RESTworld.AspNetCore, RESTworld.Business, RESTworld.EntityFrameworkCore, RESTworld.Common, RESTworld.Client.AspNetCore), write the initial Program.cs with AddRestWorld and UseRestWorld, create appsettings.json (connection string, RESTworld section), add an Angular SPA host project with AddRestWorldWithSpaFrontend and UseRestWorldWithSpaFrontend, or wire an Aspire AppHost for local development with SQL Server, health probes and HAL-Explorer links. Mirrors the ExampleBlog sample.'
argument-hint: 'Give the solution name (e.g. MyShop), the DbContext name, whether an Angular frontend and an Aspire AppHost are wanted, and the target framework (ExampleBlog uses net10.0).'
---

# RESTworld: Bootstrap a New Solution

## When to Use
- Starting a brand-new API (and optionally Angular UI) on RESTworld
- You need the project layout, package references, `Program.cs`, `appsettings.json` and Aspire wiring in one place

After the skeleton runs, add resources with [restworld-add-entity-endpoint](../restworld-add-entity-endpoint/SKILL.md). The reference implementation is `src/Example/ExampleBlog` in the RESTworld repository.

## Project Layout (`MyApi` = your solution name)

| Project | SDK | References | Contains |
|---|---|---|---|
| `MyApi.Common` | `Microsoft.NET.Sdk` | NuGet `RESTworld.Common` | DTOs, enums |
| `MyApi.Data` | `Microsoft.NET.Sdk` | NuGet `RESTworld.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design` (`IncludeAssets` = `runtime; build; native; contentfiles; analyzers; buildtransitive`), project `MyApi.Common` | entities, `DbContextBase` subclass, design-time factory, migrations |
| `MyApi.Business` | `Microsoft.NET.Sdk` | NuGet `RESTworld.Business` (brings Mapperly), project `MyApi.Data` | mappers, services, authorization handlers, validators |
| `MyApi` | `Microsoft.NET.Sdk.Web` | NuGet `RESTworld.AspNetCore`, project `MyApi.Business` | `Program.cs`, custom controllers, HAL-Forms customizations |
| `MyApi.Client` *(optional)* | `Microsoft.NET.Sdk.Web` | NuGet `RESTworld.Client.AspNetCore`; Angular `.esproj` with `<ReferenceOutputAssembly>false</ReferenceOutputAssembly>` | SPA host (serves Angular, `/settings` endpoint) |
| `MyApi.Client.Angular` *(optional)* | `.esproj` (JavaScript SDK) | npm `@wertzui/ngx-restworld-client` | Angular app (see [restworld-angular-add-page](../restworld-angular-add-page/SKILL.md)) |
| `MyApi.Aspire.AppHost` *(optional)* | `Aspire.AppHost.Sdk` | projects `MyApi`, `MyApi.Client`, Angular `.esproj` | local orchestration |

Package chain (within RESTworld): `RESTworld.AspNetCore` -> `RESTworld.Business` -> `RESTworld.EntityFrameworkCore` -> `RESTworld.Common`, and `RESTworld.Client.AspNetCore` -> `RESTworld.AspNetCore`. Referencing the top package of each project is enough. Other packages: `RESTworld.Client.Net` (.NET HAL client, see [restworld-dotnet-client](../restworld-dotnet-client/SKILL.md)) and `RESTworld.Testing` (see [restworld-testing](../restworld-testing/SKILL.md)).

ExampleBlog uses `<TargetFramework>net10.0</TargetFramework>`, `<Nullable>enable</Nullable>`, and Central Package Management (`Directory.Packages.props` with `ManagePackageVersionsCentrally`). Do the same, so `PackageReference` items carry no `Version`. Optional host packages seen in ExampleBlog: `AutoMcp.OData` (MCP server, see [restworld-api-extras](../restworld-api-extras/SKILL.md)).

```powershell
dotnet new sln -n MyApi --format slnx
dotnet new classlib -n MyApi.Common ; dotnet new classlib -n MyApi.Data ; dotnet new classlib -n MyApi.Business ; dotnet new web -n MyApi
dotnet add MyApi.Common package RESTworld.Common
dotnet add MyApi.Data package RESTworld.EntityFrameworkCore ; dotnet add MyApi.Data package Microsoft.EntityFrameworkCore.Design
dotnet add MyApi.Business package RESTworld.Business
dotnet add MyApi package RESTworld.AspNetCore
dotnet add MyApi.Data reference MyApi.Common ; dotnet add MyApi.Business reference MyApi.Data ; dotnet add MyApi reference MyApi.Business
dotnet sln add MyApi.Common MyApi.Data MyApi.Business MyApi
dotnet new tool-manifest ; dotnet tool install dotnet-ef
```
(If using Central Package Management, move the versions to `Directory.Packages.props`.) Add `Nullable` enabled in every project.

## `MyApi/Program.cs` Skeleton
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var rwBuilder = builder.AddRestWorld();                       // everything RESTworld needs (OData, HAL, versioning, Swagger, health, OpenTelemetry, ...)

rwBuilder.AddDbContextFactoryWithDefaults<MyDatabase>();      // pooled factory + DB health checks
rwBuilder.MigrateDatabaseDuringStartup<MyDatabase>();         // optional

// One block per resource, see restworld-add-entity-endpoint:
// rwBuilder.AddCrudPipeline<MyDatabase, Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>();
// rwBuilder.AddCrudMapper<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto, BlogMapper>();

var app = builder.Build();

app.UseRestWorld();                                           // middleware, Swagger UI, health endpoints, controllers

await app.RunAsync();
```
`AddRestWorld()` is generic over `IHostApplicationBuilder`; `UseRestWorld()` requires the built `WebApplication`. Mind the order: add everything to the builder, then `Build()`, then `UseRestWorld()`. For the first resource, create the context and entity with [restworld-create-entity-and-dbcontext](../restworld-create-entity-and-dbcontext/SKILL.md).

Default endpoints after `UseRestWorld()`: `/` (HAL home), `/swagger`, `/health/startup`, `/health/live`, `/health/ready`.

## `MyApi/appsettings.json`
```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MyDatabase": "Server=(localdb)\\mssqllocaldb;Database=MyDatabase;integrated security=true;"
  },
  "RESTworld": {
    "MaxNumberForListEndpoint": 50,
    "Curie": "my",
    "CalculateTotalCountForListEndpoint": true,
    "DisableAuthorization": true,
    "Versioning": { "AllowQueryParameterVersioning": true, "DefaultVersion": "1.0", "ParameterName": "v" }
  }
}
```
The connection string key must equal the DbContext class name. Set `DisableAuthorization` to `false` once real authorization is configured ([restworld-authorization-handler](../restworld-authorization-handler/SKILL.md)). All options: [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md). Set a fixed port in `Properties/launchSettings.json` (ExampleBlog: `https://localhost:5432`).

## SPA Host `MyApi.Client` (optional)
`Program.cs` (identical to ExampleBlog.Client):
```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var rwBuilder = builder.AddRestWorldWithSpaFrontend();   // calls AddRestWorld() and removes the default HomeController, adds /settings
var app = rwBuilder.Build();
app.UseRestWorldWithSpaFrontend();                       // UseRestWorld() + default/static files + fallback to index.html
await app.RunAsync();
```
`appsettings.json` tells the Angular app where the API lives (the `/settings` endpoint serves `ClientSettings`):
```json
{ "RESTworld": { "ClientSettings": { "ApiUrls": [ { "Name": "MyApi", "Url": "https://localhost:5432/", "Version": 1 } ] } } }
```
`csproj`: `Microsoft.NET.Sdk.Web`, package `RESTworld.Client.AspNetCore`, plus a `ProjectReference` to the Angular `.esproj` with `<ReferenceOutputAssembly>false</ReferenceOutputAssembly>`. Create the Angular project as described in [Angular Client Setup](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-setup.md) (ExampleBlog's `Migrate-AngularSpa.ps1` migrates the old single-project layout).

## Aspire AppHost (optional)
Use the `aspire` skill in `.agents\skills` for creating, running and troubleshooting the AppHost (`aspire start`, resources, logs); only the RESTworld-specific wiring is shown. `AppHost.csproj`: `Sdk="Aspire.AppHost.Sdk/<version>"`, `<IsAspireHost>true</IsAspireHost>`, packages `Aspire.Hosting.JavaScript` (Angular), `Aspire.Hosting.SqlServer` (container DB, optional), `HealthChecks.Kubernetes.Aspire.Hosting` (`WithKubernetesHealthProbes`), project references to `MyApi`, `MyApi.Client` and the Angular `.esproj`. ExampleBlog's `AppHost.cs` (trimmed):
```csharp
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Reads ConnectionStrings:BlogDatabase from appsettings/secrets/env. Or start SQL Server in a container:
// var database = builder.AddSqlServer("SQL-Server").AddDatabase("BlogDatabase");
var database = builder.AddConnectionString("BlogDatabase");

var apiService = builder.AddProject<ExampleBlog>(nameof(ExampleBlog))
    .WithReference(database)          // injects ConnectionStrings__BlogDatabase
    .WithKubernetesHealthProbes()     // /health/startup, /health/live, /health/ready
    .WithUrls(context =>
    {
        // add HAL-Explorer and Swagger links to the Aspire dashboard
        if (!context.Resource.TryGetEndpoints(out var endpoints))
            return;

        foreach (var endpoint in endpoints)
        {
            var uri = endpoint.AllocatedEndpoint?.UriString;
            if (uri is null)
                continue;

            context.Urls.Add(new ResourceUrlAnnotation { Url = $"https://hal-explorer.com/#uri={uri}", DisplayText = $"HAL-Explorer ({endpoint.Name})" });
            context.Urls.Add(new ResourceUrlAnnotation { Url = $"{uri}/swagger", DisplayText = $"Swagger ({endpoint.Name})" });
        }
    });

var frontendService = builder.AddProject<ExampleBlog_Client>("ExampleBlog-Client")
    .WithReference(apiService)        // injects services__ExampleBlog__https__0; /settings rewrites the API URL
    .WithKubernetesHealthProbes();

builder.AddJavaScriptApp("ExampleBlog-Client-Angular", Directory.GetParent(new Exampleblog_Client_Angular().ProjectPath)!.FullName)
    .WithNpm(installCommand: "ci")
    .WithRunScript("start")
    .WithReference(frontendService)
    .WithParentRelationship(frontendService)
    .WithHttpsEndpoint(env: "PORT")
    .WithHttpHealthCheck();

await builder.Build().RunAsync();
```
(ExampleBlog additionally calls `WithMcpServer()` on the API, which needs `#pragma warning disable ASPIREMCP001`; omit it unless you use the MCP server.) Put the connection string into the AppHost's `appsettings.json` under `ConnectionStrings`. Without an Angular UI, only the API block is needed. An `aspire.config.json` next to the solution (`{"appHost":{"path":"..."}}`) lets the Aspire CLI find the AppHost.

## Verification Checklist
- [ ] `dotnet build` succeeds for the whole solution; project references follow `Common <- Data <- Business <- Api`
- [ ] `Program.cs` calls `AddRestWorld()` before `Build()` and `UseRestWorld()` after
- [ ] Connection string key = DbContext class name; database reachable
- [ ] API starts; `GET /` returns HAL links, `/swagger` loads, `/health/ready` is healthy
- [ ] (SPA) `GET /settings` on the client host returns the API URL; Angular app loads
- [ ] (Aspire) `aspire start` shows API, client and Angular resources healthy

## Reference
- [Getting Started](https://github.com/wertzui/RESTworld/blob/main/doc/getting-started.md)
- [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md)
- [Health and Operations](https://github.com/wertzui/RESTworld/blob/main/doc/health-and-operations.md)
- [Angular Client Setup](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-setup.md)
