# RESTworld

RESTworld is a framework which utilizes other common frameworks and patterns alltogether to enable easy and fast creation of a truly RESTful API.

## Used frameworks and patterns

- Entity Framework Core for data access
- ASP.Net Core for hosting
- HAL and HAL-Forms for providing hyperlinks between resources
- OData for query support on list endpoints
- AutoMapper for mapping between Entities and DTOs
- Resource based authorization
- API Versioning through media types
- Open Telemetry for monitoring

## Pipeline

The most basic pipeline has the following data flow for a request on a list endpoint:

1. Request
2. Controller selection through ASP.Net Core (optionally with versioning)
3. Query parsing through OData
4. Controller method calls business service method
5. Authorization validates and modifies the request (both optional)
6. Service validates that all migrations have been applied to the database, to protect from locks during migration.
7. Service gets the data through Entity Framework Core
8. Entity Framework Core translates the query into SQL and gets the data from the database
9. Business service translates Entities into DTOs through Automapper
10. Authorization validates and modifies the response (both optional)
11. Controller wraps the result in a HAL response
12. Result

## Usage as API developer

### Example

You can find a complete example which leverages all the features offered by RESTworld at <https://github.com/wertzui/RESTworld/tree/main/src/Example/ExampleBlog>.

### Solution structure

If your API gets the name MyApi, structure your Solution with the following Projects:

- MyApi (ASP.Net Core Web API)
  - References RESTworld.AspNetCore, MyApi.Business
  - Contains your startup logic and your custom controllers
- MyApi.Business
  - References RESTworld.Business, MyApi.Data
  - Contains your AutoMapperConfiguration and your custom services
- MyApi.Data
  - References RESTworld.EntityFrameworkCore, MyApi.Common
  - Contains your Entity Framework Core Database Model including Entities and Migrations
- MyApi.Common
  - References RESTworld.Common
  - Contains your DTOs and Enums
- MyApi.Client.Angular (optional)
  - References RESTworld.Client.AspNetCore
  - The Angular application references @wertzui/ngx-restworld-client
- MyApi.Aspire.Apphost (optional)
  - When adding a new empty Aspire project and calling it MyApi.Aspire, two projects will be created: MyApi.Aspire.AppHost and MyApi.Aspire.ServiceDefaults.
    You can delete the MyApi.Aspire.ServiceDefaults as these are already backed into RESTworld.
  - References MyApi, MyApi.Client.Angular
  - The API service needs to reference the database and the Frontend service needds to reference the API service using `.WithReference()`
  - Wires up everything for local testing.

### Configuration

Add the following to your appsettings.json in MyApi

```json
"RESTworld": {
  "MaxNumberForListEndpoint": 10, // The maximum returned on a list endpoint
  "Curie": "MyEx", // The curie used to reference all your actions
  "CalculateTotalCountForListEndpoint": true, // If you set this to true, your clients may be able to get all pages faster as they can do more parallel requests by calculating everything themself
  "DisableAuthorization": false, // For testing purposes you can set this to false
  "Versioning": { // Add this is you want to opt into versioning
    "AllowQueryParameterVersioning": false, // This will allow legacy clients who cannot version through the media-type to version through a query parameter. This is not considered REST, but here to also support such clients.
    "DefaultVersion": "2.0", // Can either be a version or "latest"
    "ParameterName":  "v" // The name of the parameter in the media-type headers and the query "Accept: application/hal+json; v=2.0" or "http://localhost/Author/42?v=2.0"
  }
}
```

When referencing an API, either through RESTworld.Client.Net, or @wertzui/ngx-restworld-client, add the following to your appsettings.json too

```json
"RESTworld": {
  "ClientSettings": {
    "ApiUrls": [
      {
        "Name": "MyApi",
        "Url": "https://myapi.example.com/",
        "Version": 42
      }
    ]
  }
```

Change your Program.cs to the following

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var rwBuilder = rwBuilder.AddRestWorld();
var services = rwBuilder.Services;

// Have a static class AutoMapperConfiguration in your MyApi.Business project
services.AddAutoMapper(AutoMapperConfiguration.ConfigureAutomapper);

// Database
services.AddDbContextFactoryWithDefaults<TDbContext>(builder.Configuration);
rwBuilder.AddODataModelForDbContext<TDbContext>();

// Optionally migrate your database to the latest version during startup
services.MigrateDatabaseDuringStartup<TDbContext>();

// Default pipeline
services.AddCrudPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Optionally you can also add versioned pipelines
services.AddCrudPipeline<TContext, TEntity, TCreateDtoV1, TGetListDtoV1, TGetFullDtoV1, TUpdateDtoV1>(new ApiVersion(1, 0), true);
services.AddCrudPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(new ApiVersion(2, 0));

// With custom service
services.AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();

// Optionally add extra validation which can run before or after saving changes
services.AddValidator<TValidator, TCreateDto, TUpdateDto, TEntity>();

// Custom controllers will automatically be picked up by the pipeline so there is no need to register them.

var app = rwBuilder.Build();

app.UseRestWorld();

await app.RunAsync();
```

When creating MyApi.Client.Angular, change your Program.cs to the following

```csharp
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var rwBuilder = builder.AddRestWorldWithSpaFrontend();

var app = rwBuilder.Build();

app.UseRestWorldWithSpaFrontend();

await app.RunAsync();
```

### Automapper

Add an AutoMapperConfiguration to your MyApi.Business project

```csharp
using AutoMapper;
using MyApi.Common.Dtos;
using MyApi.Common.Enums;
using MyApi.Data.Models;

namespace MyApi.Business;

public class AutoMapperConfiguration
{
    public void ConfigureAutomapper(IMapperConfigurationExpression config)
    {
        // A simple mapping
        config
            .CreateMap<TEntity, TDto>()
            .ReverseMap();

        // If you are using versioning, you probably need to add different mappings for different DTO versions here
        config
            .CreateMap<MyEntity, MyDto>()
            .ReverseMap();

        config
            .CreateMap<MyEntity, MyDtoV1>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ReverseMap()
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[0]))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.Name.Split(new[] { ' ' }, 2)[1]));

        // Add more mappings
    }
}
```

### Authorization

If you want to use the inbuilt authorization logic, you must implement the interface `ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>` in a class to handle your own authorization logic. You can then register it during startup in your Program.cs.

```csharp
// Concrete implementation for one service
services.AddAuthorizationHandler<MyAuthorizationHandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Generic implementation which can be used for all services
services.AddAuthorizationHandler<MyGenericAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Register a pipeline together with concrete authorization handler
services.AddRestPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, MyAuthorizationHandler>();

// Register a pipeline together with generic authorization handler
services.AddRestPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, MyGenericAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();

// With a custom service implementation and concrete authorization handler
services.AddRestPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService, MyAuthorizationHandler>();

// With a custom service implementation and generic authorization handler
services.AddRestPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService, MyGenericAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
```

To get the current user, an `IUserAccessor` is provided, which you may want to inject into your authorization handler implementation. It is automatically populated from the `HttpContext`. However no method is provided to read the user from a token, a cookie, or something else as libraries for that are already existing. In addition no login functionality is provided, as RESTworld is meant to be a framework for APIs and the API itself should relay the login functionality to any login service (like an OAuth service or something else).

### Versioning

If you want or need to version your API, this is done through the media-type headers `Accept` and `Content-Type`. The API will parse the version from the `Accept` header and always return the used version in the `Content-Type` header. It will also advertise supported versions in the `api-supported-versions` header and advertise deprecated versions in the `api-deprecated-versions` header. So if you implement a client for the API, you should always look at the `api-deprecated-versions` header and give a warning to the user of the client if a deprecated version is used.

You can configure versioning in your appsettings as shown above. If possible, I suggest that you handle versioning in your `AutomapperConfiguration` as this is the easiest place and does not require special service implementations.

However if you cannot do this, there is also the possibility to use a REST pipeline with a custom service. That way you will need to provide one service for each version, but can also do more complex logic and database access to support multiple versions.

### Health checks

Three endpoints for health checks are provided:

1. `/health/startup`
   1. This one reports healthy once every database which you have added through `services.AddDbContextFactoryWithDefaults<TContext>(Configuration)` has migrated to the latest version.
2. `/health/live`
   1. This one reports healthy as soon as the application has started. It has no checks configured upfront.
3. `/health/ready`
   1. This one reports healthy if a connection to every database which you have added through `services.AddDbContextFactoryWithDefaults<TContext>(Configuration)` can be established.

These three endpoints have been choosen to play nicely with the three Kubernetes probes `startupProbe`, `livenessProbe` and `readinessProbe`. For more information have a look at the Kubernetes documentation <https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes>.

You can add your own health checks to these endpoints by tagging them with either `"startup"`, `"live"` or `"ready"`.

### Aspire

If you want to get a nice local environment where you can see your services and database, you can use Aspire.
Simply add references to your `MyApi` and `MyApi.Client.Angular` projects and wire everything up in your `Program.cs`

```csharp
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// This will read the connection string from the appsettings.json or secrets.json file or get it from an environment variable
// In this example it is stored in appsettings.json, because it just connects to lodaldb without a password.
var database = builder.AddConnectionString("MyDatabase");

// Instead of using localDb, you can also use a SQL Server instance which is automatically created and started in a docker container.
//var database = builder.AddSqlServer("SQL-Server")
//    .AddDatabase("MyDatabase");

// Add the backend API as a service
var apiService = builder.AddProject<MyApi>("MyApi")
    // Add a reference to the database
    // because the database has the name "MyDatabase", this will inject an environment variable ConnectionStrings__MyDatabase=Server=localhost;Database=MyDatabase;Integrated Security=True
    .WithReference(database);

// Add the Frontend which hosts the Angular client
// Note that OTLP in Angular does not work with Aspire at the moment, because Aspire only accepts OTLP through gRPC
// Until this PR is merged: https://github.com/dotnet/aspire/pull/4197
var frontendService = builder.AddProject<MyApi_Client_Angular>("MyApi-Client-Angular")
    // Add a reference to the backend API
    // This will inject an environment variable services__MyApi__https__0=https://localhost:5432
    // The SettingsController from RestWorld.Client.AspNetCore will read this environment variable and replace the API URLs with the value,
    // so that the Angular client can call the API
    .WithReference(apiService);

var app = builder.Build();

await app.RunAsync();
```

### Done

That's it. Now you can start your API and use a HAL browser like <https://chatty42.herokuapp.com/hal-explorer/index.html#uri=https://localhost:5001> to browse your API.
If you are using a `launchSettings.json`, I suggest to use this as your `"launchUrl"`.

## Usage as client developer

When developing an Angular SPA, you can use the `@wertzui/ngx-restworld-client` npm package for your Angular application and the `RESTworld.Client.AspNetCore` NuGet package for hosting.

Here are some guidelines when developing your own client:

### Make use of HAL

Write your client in such a way that it will always connect to the home-endpoint (`/`) first to discover all the other endpoints. You can cache the routes to the controller enpoints for quite some time (maybe a day or for the duration of a session), but do not hardcode them! There are also a couple of libraries for different programming languages out there which support HAL. The specification can be found at <https://stateless.group/hal_specification.html>.

### Link templating

Link templating is defined in the IETF RFC 6570 at <https://datatracker.ietf.org/doc/html/rfc6570> and there are also libraries for that. Use it together with HAL.

### OData queries for list endpoints

To filter data on the List-endpoint, you can use the OData syntax. It is very powerfull and you can find a basic tutorial on the sytax at <https://www.odata.org/getting-started/basic-tutorial/#queryData>.

### Use the /new endpoint to get sensible default data

If you want you client to be able to create new objects, you might want to query the `/new` endpoint and present that data to your user so he is guided a little bit.

### Batch operations for POST and PUT

The POST (Create) and PUT (Update) endpoints both accept either a single object or an array of objects. If you need to create or modify a huge number of objects, you should send them as an array as this will ensure the atomicity of such an operation and might also give you a huge performance gain as it will save a lot of roundtrips.

### Versioning from the client perpective

Always send your supported version(s) in the `Accept` header to ensure you always get a  response that you can handle and watch for the `api-deprecated-versions` header to quickly get notified if you need to change your client before it breaks. The format of the `Accept` header should always be `application/hal+json; v=42` (or whatever version you need and is offered by the server).
