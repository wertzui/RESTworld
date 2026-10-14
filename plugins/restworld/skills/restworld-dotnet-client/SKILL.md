---
name: restworld-dotnet-client
description: 'Consume a RESTworld API from .NET code with RESTworld.Client.Net, or host an Angular SPA next to the API with RESTworld.Client.AspNetCore. Use when asked to call a RESTworld API from a worker, console app, MVC/Minimal API or another service; to register AddRestWorldClients; to configure RESTworld:ClientSettings:ApiUrls (Name, Url, Version); to use IRestWorldClient / IRestWorldClientCollection (GetClient, GetListAsync, GetAllPagesListAsync, GetFormListAsync, GetLinkFromHome); to follow HAL links (FollowGetAsync, FollowPostAsync, FollowPutAsync), page through OData lists, create/update/batch-create resources, handle If-Match/Timestamp conflicts or deprecated API versions; or to set up the SPA host with AddRestWorldWithSpaFrontend / UseRestWorldWithSpaFrontend and the SettingsController.'
argument-hint: 'Describe what the .NET consumer must do (list, read, create, update, batch) and which API/DTO types it uses. Optionally name the host type (worker, ASP.NET Core app, SPA host).'
---

# RESTworld: .NET Client and SPA Host

Two packages:
- **`RESTworld.Client.Net`** — typed access to RESTworld (HAL / HAL-Forms) APIs from any .NET app. Built on `HAL.Client.Net`.
- **`RESTworld.Client.AspNetCore`** — an ASP.NET Core host that serves an Angular SPA and the client settings (see below).

## When to Use
- A .NET service must read or write data in a RESTworld API
- You need to configure API URLs and versions for a client
- You are setting up the ASP.NET Core host for the Angular frontend (see also [Angular client docs](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular.md))

## Key Rules

1. **Discover, don't hardcode.** `RestWorldClient` loads the API's home resource (`/`) once at startup and finds URLs via links. Resolve list endpoints with `GetLinkFromHome(rel, name, curie)`.
2. **Link relations** on the home resource are `"<curie>:<ControllerName>"` (e.g. `MyEx:Blog`, curie from `RESTworld:Curie`; default is the upper-case letters of the entry assembly lower-cased). The link `Name` is the action: `RESTworld.Common.Constants` has `GetListLinkName` (`"GetList"`), `GetLinkName`, `PostLinkName`, `PutLinkName`, `DeleteLinkName`, `NewLinkName`, and `Constants.ListItems` (`"items"`) for `Embedded`. If you omit the curie, the client uses the one from the home resource's `curies` link.
3. **Version**: `ApiUrl.Version` is sent in the `Accept` header (`v=<n>`) for home discovery. Pass `version:` to later calls explicitly. The client logs a warning for deprecated and an error for unsupported versions at startup.
4. **Links may be URI templates** (RFC 6570, e.g. `{id}`, `{?$filter,$orderby,$top,$skip}`). Pass values via `uriParameters` (`IDictionary<string, object>`); keys may contain `$`.
5. **Optimistic concurrency**: updates (`TUpdateDto : ConcurrentDtoBase`) must carry the `Timestamp` of the resource you read. A mismatch returns `409 Conflict`. Delete takes the timestamp as `If-Match` header or `timestamp` query value (the `delete` link on a resource already includes it).
6. **Always check `response.Succeeded`** (`HalResponse<T>`: `Resource`, `StatusCode`, `ProblemDetails`). Use `EnsureSuccessStatusCode()` if you prefer exceptions.
7. **`AddRestWorldClients` does not bind configuration to `RestWorldClientOptions`** (an ASP.NET Core app that called `AddRestWorld()` has it bound already). In a plain worker/console host, bind it yourself or the collection is created empty (log: *"do not contain any ApiUrls"*).

## Steps

### 1. Configure (`appsettings.json`)
```json
{
  "RESTworld": {
    "ClientSettings": {
      "ApiUrls": [ { "Name": "ExampleBlog", "Url": "https://localhost:5432/", "Version": 2 } ]
    }
  }
}
```
`Name` must be unique (duplicates throw at startup). A trailing `/` is added to `Url` automatically.

### 2. Register
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.Client.Net;
using RESTworld.Common.Client;

var builder = Host.CreateApplicationBuilder(args);

// Needed in plain hosts (not needed if the app already called AddRestWorld()).
builder.Services.Configure<RestWorldClientOptions>(builder.Configuration.GetSection("RESTworld"));

builder.AddRestWorldClients(new Dictionary<string, Action<IServiceProvider, HttpClient>?>
{
    ["ExampleBlog"] = (sp, http) => http.DefaultRequestHeaders.Authorization = new("Bearer", "<token>"),
});
var host = builder.Build();

var client = host.Services.GetRequiredService<IRestWorldClientCollection>().GetClient("ExampleBlog");
```
`AddRestWorldClients(this IHostApplicationBuilder, IDictionary<string, Action<IServiceProvider, HttpClient>?>? clientConfigurations = null)` registers the HAL client factory (one named `HttpClient` per `ApiUrls` entry) and a singleton `IRestWorldClientCollection` (`GetClient(name)` throws `ArgumentException` if unknown, `TryGetClient(name, out client)`). The collection is built synchronously on first resolve and **throws** if any home endpoint is unreachable.

### 3. List (OData, paging)
```csharp
using HAL.Common;
using Constants = RESTworld.Common.Constants;   // HAL.Common also has a Constants class

var response = await client.GetAllPagesListAsync(
    "Post",                                             // controller name, curie is resolved automatically
    uriParameters: new Dictionary<string, object> { ["$filter"] = "state eq 'Published'", ["$orderby"] = "headline" },
    version: "2");

if (!response.Succeeded) { /* response.StatusCode, response.ProblemDetails */ return; }

foreach (var item in response.Resource!.Embedded![Constants.ListItems])
{
    dynamic state = ((dynamic)item).State;      // untyped lists deserialize the state dynamically
    Console.WriteLine($"{state.id}: {state.headline}");
}
```
- `GetListAsync` returns one page (`$skip`/`$top` via `uriParameters`; the server caps `$top` at `RESTworld:MaxNumberForListEndpoint`). `GetAllPagesListAsync` follows `next` links and merges items. `GetFormListAsync` / `GetAllPagesFormListAsync` return `FormsResource<Page>` including the `Search`/`Edit` templates.
- Enum values in `$filter` use the OData form `'Published'`.

### 4. Read, update, delete via links
```csharp
var self = item.FindLink("self")!;                                   // HAL.Common.ResourceExtensions
var full = await self.FollowGetAsync<PostGetFullDto>(client, version: "2");

var dto = full.Resource!.State!;
var save = full.Resource.FindLink("save")!;                          // present when the API returns plain HAL
var updated = await save.FollowPutAsync<PostUpdateDto, PostGetFullDto>(
    client,
    new PostUpdateDto { Id = dto.Id, Timestamp = dto.Timestamp, Headline = "New", Text = dto.Text, AuthorId = dto.AuthorId, BlogId = dto.BlogId },
    version: "2");

if (updated.StatusCode == System.Net.HttpStatusCode.Conflict) { /* reload and retry */ }
```
`Link` extension methods (`HAL.Client.Net.LinkExtensions`, namespace `HAL.Common`): `FollowGetAsync`, `FollowPostAsync`, `FollowPutAsync`, `FollowDeleteAsync` and `…FormAsync` variants. They take the `IHalClient` (an `IRestWorldClient` is one) first.

### 5. Create and batch
```csharp
var newLink  = client.GetLinkFromHome("Blog", Constants.NewLinkName)!;   // GET …/new returns an empty template with defaults
var template = await newLink.FollowGetAsync<BlogCreateDto>(client, version: "2");

var postLink = client.GetLinkFromHome("Blog", Constants.PostLinkName)!;
var created  = await postLink.FollowPostAsync<BlogCreateDto, BlogGetFullDto>(client, new BlogCreateDto { Name = "x" }, version: "2");

// Batch: POST/PUT also accept an array in one request.
var batch = await postLink.FollowPostAsync<IReadOnlyCollection<BlogCreateDto>>(
    client, [new BlogCreateDto { Name = "a" }, new BlogCreateDto { Name = "b" }], version: "2");
```
`DELETE` is single-resource only (`id` + timestamp). For partial changes the server also accepts `PATCH` with `application/json-patch+json` — see skill `restworld-api-extras`.

### 6. SPA host (`RESTworld.Client.AspNetCore`)
`ExampleBlog.Client/Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
var rwBuilder = builder.AddRestWorldWithSpaFrontend();      // calls AddRestWorld(), removes the default HomeController, adds SettingsController
var app = rwBuilder.Build();
app.UseRestWorldWithSpaFrontend();                          // UseRestWorld + default files + static files + fallback to index.html
await app.RunAsync();
```
`AddRestWorldWithSpaFrontend<TBuilder>(this TBuilder builder, string contentRoot = "ClientApp/dist")` and `UseRestWorldWithSpaFrontend<TApplication>(this TApplication app)` (namespace `Microsoft.AspNetCore.Builder`). The `SettingsController` serves `GET /settings` → the `RESTworld:ClientSettings` section (`ApiUrls` + `Extensions`) so the Angular app can find its APIs; OpenTelemetry values from configuration are added to `Extensions`, and API URLs are overridden by .NET service discovery when present (Aspire). A custom controller named `SettingsController` replaces the built-in one. Add the same `RESTworld:ClientSettings` block to this host's `appsettings.json`.

## Verification Checklist
- [ ] `RESTworld:ClientSettings:ApiUrls` has unique `Name`s, correct `Url` and `Version`
- [ ] `RestWorldClientOptions` is bound (automatic only in hosts that called `AddRestWorld()`)
- [ ] Every response checked with `Succeeded`; `409` handled for updates/deletes
- [ ] `Timestamp` copied from the last read into update DTOs
- [ ] Auth header configured via the `AddRestWorldClients` dictionary when the API requires it
- [ ] SPA host: `appsettings.json` contains `ClientSettings`, `/settings` returns it

## Related
- [.NET client guide](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-dotnet.md) · [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md) · [Angular client](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular.md)
- Skills `restworld-angular-add-page`, `restworld-api-extras`
