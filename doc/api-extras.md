# API Extras

Small features that cut across all RESTworld endpoints. Each section says whether the feature is automatic or needs configuration.

## Swagger / OpenAPI (automatic)

`AddRestWorld()` registers Swashbuckle and ASP.NET Core OpenAPI; `UseRestWorld()` serves the Swagger document, Swagger UI and the OpenAPI endpoint. The home resource links to the UI.

- There is one document per API version. Swagger UI lists them newest first. Versions are requested through the `v` parameter of the media type (`application/hal+json; v=2`). Query-string versioning is only enabled with `RESTworld:Versioning:AllowQueryParameterVersioning` and is not advertised in Swagger.
- RESTworld hides the OData query option bodies, describes JSON Patch documents as arrays of RFC 6902 operations and removes history endpoints for DTOs without `[HasHistory]`.
- To hide a controller action parameter from Swagger put `[SwaggerIgnore]` (`RESTworld.AspNetCore.Swagger`) on it.

## MCP server with AutoMcp (opt-in)

RESTworld controllers can be exposed to AI assistants as [Model Context Protocol](https://modelcontextprotocol.io) tools with the `AutoMcp.OData` package (pre-release `1.0.0-beta-3` in the example). From `ExampleBlog/Program.cs`:

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithOData()       // before WithAutoMcp()
    .WithAutoMcp();

var app = builder.Build();
app.UseRestWorld();
app.MapMcp("mcp");     // /mcp
```

Every controller action becomes a tool. AutoMcp uses `[Description]` and `[ProducesResponseType]`, so describe the actions of your custom controllers with `[Description("…")]`.

## CSV output (automatic)

List and history endpoints answer `Accept: text/csv` with the items of `_embedded.items` as CSV (UTF-8 unless another encoding is requested; the culture follows the request localization). Single-resource endpoints do not offer CSV.

## JSON Patch (automatic on CRUD controllers)

`PATCH /{resource}` with `Content-Type: application/json-patch+json` can create and update in one request:

```json
[
  { "op": "add",     "path": "/-",      "value": { "name": "New blog" } },
  { "op": "replace", "path": "/3/name", "value": "Renamed" }
]
```

- `/-` creates a resource (the value is a create DTO). `/{id}/{property}` addresses the update DTO of the existing resource.
- Create and update authorization handlers run only for the kinds of operations in the patch. The id cannot be patched.
- An invalid path, a missing id (404), or a failing `test` operation (400) makes the whole request fail without writing anything.
- The response contains the created resources followed by the updated ones.
- In .NET: `new JsonPatchDocument<TCreateDto, TUpdateDto> { SerializerOptions = JsonSerializerOptions.Web }.Replace(id, d => d.Name, "x").Create(new TCreateDto { … })` (`RESTworld.Business.Models`).

## Caching (configurable)

Results of `Get`, `GetList` and `GetHistory` are cached in memory per user for a short time. Successful writes through the CRUD controller evict the affected entries.

```json
"RESTworld": {
  "Caching": { "Get": "00:00:30", "GetList": "00:00:05", "GetHistory": "00:00:10" }
}
```

The default for each is 10 seconds. In custom controllers use `ICacheHelper`, e.g. `Cache.GetOrCreateWithCurrentUserAsync(key, nameof(CachingOptions.Get), factory)`; the timeout key must exist in `CachingOptions`.

## Concurrency and timestamps (automatic)

- Entities derived from `ConcurrentEntityBase` or `ChangeTrackingEntityBase` have a `[Timestamp] byte[]? Timestamp`; update DTOs derive from `ConcurrentDtoBase`. When you save through `DbContextBase`, the timestamp sent by the client is compared with the database.
- A mismatch yields **409 Conflict** ("Concurrency validation failed. Please reload the resource."). Foreign-key violations also yield 409.
- `DELETE /{resource}/{id}` requires the timestamp, either in the `If-Match` header (preferred) or as `?timestamp=` (base64url). The `delete` link of a resource already contains it.
- `ChangeTrackingEntityBase` also fills `CreatedAt`, `CreatedBy`, `LastChangedAt` and `LastChangedBy` when saving with the current user.

## History (needs `[HasHistory]` and a temporal table)

```csharp
[HasHistory]
public class PostGetFullDto : ChangeTrackingDtoBase { … }

// DbContext
modelBuilder.Entity<Post>(e => e.ToTable(b => b.IsTemporal()));
```

`GET /{resource}/history` accepts the OData options plus `$at`, or `$from` with `$to` or `$toInclusive`. `$at` excludes the other three; `$to` and `$toInclusive` exclude each other. Without the attribute the endpoint returns 404 ("This resource does not have a history.").

## Images and uploads (DTO attributes)

`[RestWorldImage(...)]` on a `HalFile?` property configures the image cropper of the Angular client (aspect ratio, output size and format, quality, minimum/maximum crop size, rounded cropper, background color and more). File uploads without cropping use `[DataType(DataType.Upload)]`.

```csharp
[RestWorldImage(AspectRatio = 120.0 / 40.0, ContainWithinAspectRatio = true, MaintainAspectRatio = true,
                OnlyScaleDown = false, ResizeToHeight = 40, ResizeToWidth = 120)]
public HalFile? Image { get; set; }

[DataType(DataType.Upload)]
public HalFile? Attachement { get; set; }
```

Your service stores and loads the `HalFile` content (see `PostService` in the example).

## Health endpoints (automatic)

`UseRestWorld()` maps `/health/startup`, `/health/live` and `/health/ready` and the home resource links to them. `AddDbContextFactoryWithDefaults<TContext>()` adds a migration check (tag `startup`) and a connection check (tag `ready`). Tag your own checks `startup`, `live` or `ready` to include them. See [Health and Operations](health-and-operations.md).

## OpenTelemetry (automatic)

`AddRestWorld()` configures OpenTelemetry logging, tracing (ASP.NET Core, HttpClient, EF Core, SqlClient) and metrics (ASP.NET Core, HttpClient, runtime). Health probes and CORS preflight requests are excluded from traces. Telemetry is exported over OTLP when the configuration value `OTEL_EXPORTER_OTLP_ENDPOINT` is set (Aspire sets it). The OTEL values are also passed to the Angular frontend through `RESTworld:ClientSettings:Extensions`.

Related: [Configuration](configuration.md), [DTO Types and Roles](dto-types.md), [.NET Client Development](client-development-dotnet.md).
