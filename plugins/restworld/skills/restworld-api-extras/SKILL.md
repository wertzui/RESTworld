---
name: restworld-api-extras
description: 'Use the small cross-cutting features of a RESTworld API. Use when asked about Swagger / OpenAPI / Swagger UI (also per API version), exposing the API as an MCP server (AutoMcp, AddMcpServer, WithAutoMcp, WithOData, MapMcp), CSV export (text/csv), JSON Patch (PATCH application/json-patch+json, create and update in one request), caching options (RESTworld:Caching Get/GetList/GetHistory), concurrency handling (Timestamp, If-Match, 409 Conflict), history endpoints / temporal tables (HasHistory attribute, $at/$from/$to), the RestWorldImage attribute for image upload and cropping, health endpoints (/health/startup, /health/live, /health/ready), OpenTelemetry/OTLP setup, or hiding a parameter from Swagger (SwaggerIgnore). Also handles: telling which of these features is automatic and which needs configuration.'
argument-hint: 'Name the feature you need (e.g. "MCP", "history", "caching", "JSON Patch"). Optionally describe the DTO/entity involved.'
---

# RESTworld: API Extras

Each section lists what RESTworld does **automatically** after `builder.AddRestWorld()` / `app.UseRestWorld()` and what you have to add. Everything below was checked against the RESTworld source and `src/Example/ExampleBlog`.

## When to Use
You need one of: Swagger, MCP, CSV output, JSON Patch, cache tuning, concurrency handling, entity history, image fields, health probes, or OpenTelemetry — and want the exact switch.

## Swagger / OpenAPI — automatic
- `AddRestWorld()` registers Swashbuckle (`AddSwaggerGen`) and `AddOpenApi`; `UseRestWorld()` calls `UseSwagger()`, `UseSwaggerUI()` and `MapOpenApi()`. Swagger UI is at `/swagger` (ASP.NET Core default prefix) and the home resource links to it.
- **One document per API version** (`ConfigureVersioningWithSwaggerOptions`, title `"<entry assembly> <version>"`); Swagger UI lists them newest first. Version is selected by the `v` parameter of the `Accept` media type; query-string versioning appears only if `RESTworld:Versioning:AllowQueryParameterVersioning` is `true` (not advertised in Swagger).
- RESTworld's filters hide OData query option bodies, describe `JsonPatchDocument` as an array of RFC 6902 operations, and remove history endpoints for DTOs without `[HasHistory]`. Large responses (> ~1 MB) are downloaded instead of rendered in the UI.
- Hide a controller action parameter: `[SwaggerIgnore]` (`RESTworld.AspNetCore.Swagger.SwaggerIgnoreAttribute`, parameters only).

## MCP server (AutoMcp) — opt-in
`ExampleBlog.csproj` references `AutoMcp.OData` (1.0.0-beta-3, pulls in `AutoMcp` and `ModelContextProtocol.AspNetCore`). `Program.cs`:
```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithOData()       // must come before WithAutoMcp()
    .WithAutoMcp();    // every controller action becomes an MCP tool

var app = builder.Build();
app.UseRestWorld();
app.MapMcp("mcp");     // endpoint /mcp
```
AutoMcp reads `[Description]` and `[ProducesResponseType]` from actions, so describe custom controller actions with `[Description("…")]` (RESTworld's own actions already do). The package is a pre-release.

## CSV output — automatic
`AddRestWorld()` adds `CsvOutputFormatter`. List and history endpoints accept `Accept: text/csv` and return the `State` of each item in `_embedded.items` as CSV (UTF-8 by default; culture from the request localization). Single-resource endpoints do not offer CSV.

## JSON Patch — automatic on CRUD controllers
`PATCH /{resource}` with `Content-Type: application/json-patch+json` creates and/or partially updates in one request (`CrudController.PatchAsync`, body type `RESTworld.Business.Models.JsonPatchDocument<TCreateDto, TUpdateDto>`):
```json
[
  { "op": "add",     "path": "/-",          "value": { "name": "New blog" } },
  { "op": "replace", "path": "/3/name",     "value": "Renamed" }
]
```
- `/-` creates (value is a `TCreateDto`), `/{id}/{property}` addresses the update DTO. Operations `add`, `remove`, `replace`, `test`, `move`, `copy` are supported.
- Create/update authorization handlers are called only for the kinds of operations used. The id cannot be patched. A failing `test`, an invalid path or a missing id produces 400/404 and **nothing is written**. The response lists created resources followed by updated ones.
- Typed builder for .NET callers: `new JsonPatchDocument<TCreate, TUpdate> { SerializerOptions = JsonSerializerOptions.Web }.Replace(id, d => d.Name, "x").Create(new TCreate { … })`.

## Caching — configurable
Controller results of `Get`, `GetList` and `GetHistory` are cached in memory **per user** (`CacheHelper`); successful writes through the CRUD controller evict the affected entries.
```json
"RESTworld": { "Caching": { "Get": "00:00:30", "GetList": "00:00:05", "GetHistory": "00:00:10" } }
```
Defaults are 10 s each (`CachingOptions`, a `Dictionary<string, TimeSpan>`). In custom controllers use `ICacheHelper` and a key present in the options, e.g. `Cache.GetOrCreateWithCurrentUserAsync(key, nameof(CachingOptions.Get), factory)` (an unknown key throws `ArgumentException`).

## Concurrency (Timestamp) — automatic with the base classes
- Entities derive from `ConcurrentEntityBase`/`ChangeTrackingEntityBase` (`[Timestamp] byte[]? Timestamp`), update DTOs from `ConcurrentDtoBase`. `DbContextBase.SaveChangesAsync` uses `TimestampConcurrencyDetection` so the **timestamp sent by the client** is compared with the database.
- Mismatch → `DbUpdateConcurrencyException` → **409 Conflict** with the message *"Concurrency validation failed. Please reload the resource."*. The same status is used for foreign-key violations.
- `DELETE /{resource}/{id}` needs the timestamp: header `If-Match` (preferred) or query `?timestamp=` (base64url). The `delete` link in HAL responses already contains it.
- The timestamp travels inside the DTO body; no `ETag` response header is produced by RESTworld.
- `ChangeTrackingEntityBase` also fills `CreatedAt/By` and `LastChangedAt/By` on save (the user comes from `IUserAccessor`).

## History — needs `[HasHistory]` and a temporal table
Mark the `TGetFullDto`: `[HasHistory] public class PostGetFullDto : ChangeTrackingDtoBase`. Make the table temporal (SQL Server system-versioned): `modelBuilder.Entity<Post>(e => e.ToTable(b => b.IsTemporal()))` (as in `BlogDatabase`).
`GET /{resource}/history` supports the usual OData options plus `$at`, or `$from` with `$to` or `$toInclusive` (not both `$to` and `$toInclusive`; `$at` excludes the others). Without the attribute the endpoint answers 404 *"This resource does not have a history."* Resources of such DTOs get a `history` link.

## Images and file uploads — DTO attributes
`[RestWorldImage(AspectRatio = 120.0/40.0, ResizeToWidth = 120, ResizeToHeight = 40, MaintainAspectRatio = true, …)]` on a `HAL.Common.Binary.HalFile?` property configures the Angular image cropper (`Accept`, `AlignImage`, `AspectRatio`, `BackgroundColor`, `Format`, `ImageQuality`, `CropperMinWidth`, `OnlyScaleDown`, `RoundCropper` and more; it is a `DataTypeAttribute` with `DataType.ImageUrl`). Plain uploads: `[DataType(DataType.Upload)] public HalFile? Attachement { get; set; }`. See `PostGetFullDto` in ExampleBlog; the service must persist/load the `HalFile` itself (see `PostService`).

## Health endpoints — automatic
`UseRestWorld()` maps `/health/startup`, `/health/live`, `/health/ready` (HAL-formatted JSON) and the home resource links to them. `AddDbContextFactoryWithDefaults<TContext>()` adds a *migration* check (tag `startup`) and a *connection* check (tag `ready`). Add your own checks with `services.AddHealthChecks().AddCheck<T>("name", tags: ["ready"])`. More in [health-and-operations.md](https://github.com/wertzui/RESTworld/blob/main/doc/health-and-operations.md).

## OpenTelemetry — automatic, endpoint opt-in
`AddRestWorld()` configures OpenTelemetry logging, tracing (ASP.NET Core, HttpClient, EF Core, SqlClient) and metrics (ASP.NET Core, HttpClient, runtime). Health probes and CORS preflights are filtered out of traces. Exporting is enabled when the configuration key `OTEL_EXPORTER_OTLP_ENDPOINT` is set (the standard OTLP variables apply; Aspire sets them). The OTEL values are also copied to `RESTworld:ClientSettings:Extensions` for the Angular frontend.

## Verification Checklist
- [ ] Feature needing code (MCP, `[HasHistory]`, `[RestWorldImage]`, `[SwaggerIgnore]`) added, others only configured
- [ ] MCP: `WithOData()` precedes `WithAutoMcp()`; `MapMcp("mcp")` after `Build()`
- [ ] History: temporal table + `[HasHistory]` on the full DTO
- [ ] Caching keys limited to `Get`, `GetList`, `GetHistory` (or ones you add yourself)
- [ ] Clients send `Timestamp` back (updates) or `If-Match` (delete)

## Related
- [API extras guide](https://github.com/wertzui/RESTworld/blob/main/doc/api-extras.md) · [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md) · [Health and operations](https://github.com/wertzui/RESTworld/blob/main/doc/health-and-operations.md)
- Skills `restworld-custom-controller`, `restworld-dotnet-client`, `restworld-add-api-version`
