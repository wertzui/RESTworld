# .NET Client Development

Use the `RESTworld.Client.Net` package to integrate RESTworld APIs into .NET workers, background services, or MVC/Minimal API apps. The client library sits on top of the HAL.NET stack, giving you typed helpers for HAL discovery, list pagination, and form submissions.

## Core RESTworld concepts

### Start with HAL discovery

Always call the home endpoint (`/`) before requesting specific resources. Cache discovered routes per session or per day, but avoid hardcoding URLs. Refer to the HAL specification at <https://stateless.group/hal_specification.html> for link semantics.

### Link templating

RESTworld links may include URI templates. Follow RFC 6570 (<https://datatracker.ietf.org/doc/html/rfc6570>) and use helpers such as `Link.FollowGetAsync` to expand templates safely.

### OData queries for list endpoints

List endpoints support the OData query language, allowing filtering, sorting, and pagination on the server side. If you're new to the syntax, start with the <https://www.odata.org/getting-started/basic-tutorial/#queryData> tutorial.

### Using the `/new` endpoint

When creating new entities, retrieve the `/new` form or resource first to obtain sensible defaults. This keeps DTO construction aligned with server expectations.

### Batch operations

`POST` (create) and `PUT` (update) endpoints accept either single objects or arrays. Sending batches reduces round trips and keeps operations atomic.

### Versioning from the client perspective

Always advertise supported versions in the `Accept` header, e.g., `application/hal+json; v=42`. Inspect the `api-deprecated-versions` header in responses and alert users when they rely on a deprecated version. Review [Mapping and Versioning](mapping-and-versioning.md) to understand how the server processes these headers.

## Registering RESTworld clients

Configure clients via `appsettings.json` as described in [Configuration](configuration.md) and call `AddRestWorldClients` on your `HostApplicationBuilder`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.Client.Net;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRestWorldClients(new Dictionary<string, Action<IServiceProvider, HttpClient>?>
{
    ["ExampleBlog"] = (services, http) =>
    {
        // Optional: decorate the HttpClient, e.g., add auth headers or resilience policies.
        // var tokenProvider = services.GetRequiredService<ITokenProvider>();
        // http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenProvider.GetToken());
    }
});

var host = builder.Build();
await host.StartAsync();
```

`AddRestWorldClients` wires up `IHalClientFactory`, reads `RESTworld:ClientSettings:ApiUrls`, and registers a singleton `IRestWorldClientCollection`. Use dependency injection to resolve it:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RESTworld.Common.Client;

var clients = host.Services.GetRequiredService<IRestWorldClientCollection>();
var exampleBlog = clients.GetClient("ExampleBlog");
```

## Calling list, edit, and form endpoints

The `IRestWorldClient` interface exposes helpers for common HAL patterns. For example, fetch all posts while auto-following pagination:

```csharp
using HAL.Common;
using RESTworld.Common;
using RESTworld.Common.Dtos;

var response = await exampleBlog.GetAllPagesListAsync("Post", curie: "MyEx");
if (!response.Succeeded || response.Resource?.Embedded is null)
{
    // handle errors (response.StatusCode, response.Details...)
    return;
}

if (response.Resource.Embedded.TryGetValue(Constants.ListItems, out var posts) && posts is not null)
{
    foreach (var post in posts)
    {
        var dto = post.State as PostGetFullDto;
        Console.WriteLine($"{dto?.Title} ({dto?.Id})");
    }
}
```

Submit a new resource by following the `create` form and posting the payload:

```csharp
var href = client.GetLinkFromHome("Blog", "Post", "MyEx");
var uri = new Uri(href);
var createForm = await client.GetFormAsync<BlogCreateDto>(uri);

var template = createForm.Templates["_default"];
var target = template.target;
var newBlog = createForm.State;
newBlog.Title = "A very good title";

var response = await exampleBlog.SendFormAsync<BlogCreateDto, BlogGetFullDto>(
    template.Method,
    new Uri(template.Target),
    newBlog
    );

if (response.Succeeded)
{
    var createdBlog = result.Resource.State;
    Console.WriteLine($"Created blog with id {createdBlog.Id}");
}
```

## Working with link metadata

- `GetLinkFromHome` returns a single link for a relation. Use the optional `curie` argument when the relation is not namespaced.
- `GetLinksFromHome` returns all links matching a relation.
- `GetAllPagesFormListAsync` aggregates paginated form responses, allowing bulk updates.

These helpers let you build clients that respond to hypermedia changes without redeploying.

## Hosting patterns

- **Console/Worker services:** Resolve `IRestWorldClientCollection` inside a `BackgroundService` and keep the host running with `await host.RunAsync()`.
- **ASP.NET Core apps:** Call `builder.AddRestWorldClients()` in `Program.cs` and inject `IRestWorldClientCollection` into your services.
- **Multi-tenant or multi-environment setups:** Define multiple entries under `RESTworld:ClientSettings:ApiUrls` and select the desired client by name.

For Angular guidance see [Angular Client Development](client-development-angular.md). Return to [Getting Started](getting-started.md) for server-side setup or [Health and Operations](health-and-operations.md) for deployment practices.
