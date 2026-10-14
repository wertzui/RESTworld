# Authorization

RESTworld ships with extensibility points for resource-based authorization. Implement handlers to enforce access rules before and after business logic runs.

## How handlers work

Services call every registered handler **before** the business logic (`Handle...RequestAsync`) and **after** it (`Handle...ResponseAsync`):

- A request handler receives the previous `AuthorizationResult<TEntity, ...>` and returns it, modified or not. If the returned `Status` is not `200 OK` (for example `401` or `403`), the call stops and that status is returned to the client.
- The `Filter` of the result is applied to the `IQueryable<TEntity>` that is used for reading, updating and deleting. Entities that are filtered out behave as if they did not exist.
- A response handler receives the `ServiceResponse<T>` and returns it, a modified DTO, or a different response.
- Request handlers are chained in registration order. If no handler is registered, no authorization is performed and a warning is logged once.

## Implementing handlers

Handlers live in `RESTworld.Business.Authorization`. Derive from a base class whose methods pass everything through by default and override only what you need.

| Used with | Interface | Base class | Base class with authenticated-user check |
|---|---|---|---|
| CRUD pipeline | `ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>` | `CrudAuthorizationHandlerBase<...>` | `UserIsAuthorizedCrudAuthorizationHandler<...>` |
| Read-only pipeline | `IReadAuthorizationHandler<TEntity, TQueryDto, TGetListDto, TGetFullDto>` | `ReadAuthorizationHandlerBase<...>` | `UserIsAuthorizedReadAuthorizationHandler<...>` |
| Custom service | `IBasicAuthorizationHandler<TRequest, TResponse>` | `BasicAuthorizationHandlerBase<TRequest, TResponse>` | `UserIsAuthorizedBasicAuthorizationHandler<TRequest, TResponse>` |
| Custom service with database filter | `IBasicAuthorizationHandler<TEntity, TRequest, TResponse>` | `BasicAuthorizationHandlerBase<TEntity, TRequest, TResponse>` | `UserIsAuthorizedBasicAuthorizationHandler<TEntity, TRequest, TResponse>` |

`ICrudAuthorizationHandler` combines `ICreateAuthorizationHandler`, `IReadAuthorizationHandler`, `IUpdateAuthorizationHandler` and `IDeleteAuthorizationHandler`, so one handler covers every operation of a CRUD pipeline. There is also an `IBasicAuthorizationHandler<TResponse>` for services without a request parameter; it has no base class.

### CRUD and read handlers

| Operation | Request method | Response method |
|---|---|---|
| Create | `HandleCreateRequestAsync` (single `TCreateDto` and `IReadOnlyCollection<TCreateDto>`) | `HandleCreateResponseAsync` (`TGetFullDto` and `IReadOnlyCollection<TGetFullDto>`) |
| Get list | `HandleGetListRequestAsync` | `HandleGetListResponseAsync` |
| Get single | `HandleGetSingleRequestAsync` (`AuthorizationResult<TEntity, long>`) | `HandleGetSingleResponseAsync` |
| Get history | `HandleGetHistoryRequestAsync` | `HandleGetHistoryResponseAsync` |
| Update | `HandleUpdateRequestAsync` (single `TUpdateDto` and `IUpdateMultipleRequest<TUpdateDto, TEntity>`) | `HandleUpdateResponseAsync` (single and collection) |
| Delete | `HandleDeleteRequestAsync` (`AuthorizationResult<TEntity, long, byte[]>`: id and timestamp) | `HandleDeleteResponseAsync` (`ServiceResponse<object>`) |

Every method takes a `CancellationToken` as last parameter. The history methods of `ReadAuthorizationHandlerBase` are not virtual and just pass through.

The ExampleBlog's `BlogpostAuthorizationHandler` filters list results, answers single gets with an arbitrary status and replaces one response:

```csharp
public class BlogpostAuthorizationHandler : CrudAuthorizationHandlerBase<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    // Only posts with an even ID are returned.
    public override Task<AuthorizationResult<Post, IGetListRequest<Post, PostQueryDto, PostListDto>>> HandleGetListRequestAsync(
        AuthorizationResult<Post, IGetListRequest<Post, PostQueryDto, PostListDto>> previousResult, CancellationToken cancellationToken)
        => Task.FromResult(previousResult.WithFilter(source => source.Where(p => p.Id % 2 == 0)));

    // Requesting an ID that is also an HTTP status code returns that status.
    public override Task<AuthorizationResult<Post, long>> HandleGetSingleRequestAsync(
        AuthorizationResult<Post, long> previousResult, CancellationToken cancellationToken)
    {
        var requestedPostId = previousResult.Value1;
        if (Enum.IsDefined(typeof(HttpStatusCode), (int)requestedPostId))
            return Task.FromResult(AuthorizationResult.FromStatus<Post, long>((HttpStatusCode)requestedPostId, requestedPostId));

        return Task.FromResult(previousResult);
    }

    // The response for the post with ID 42 is replaced by a problem.
    public override Task<ServiceResponse<PostGetFullDto>> HandleGetSingleResponseAsync(
        ServiceResponse<PostGetFullDto> previousResponse, CancellationToken cancellationToken)
    {
        if (previousResponse.ResponseObject?.Id == 42)
            return Task.FromResult(ServiceResponse.FromProblem<PostGetFullDto>((HttpStatusCode)42, "This is the answer!"));

        return Task.FromResult(previousResponse);
    }
}
```

### Handlers for custom services

`MyCustomAuthorizationHandler` guards the `MyCustomService` behind a [custom controller](custom-controllers.md). It derives from `BasicAuthorizationHandlerBase<Post, long, PostWithAuthorDto>` (entity, request, response):

```csharp
public override Task<AuthorizationResult<Post, long>> HandleRequestAsync(AuthorizationResult<Post, long> previousResult, CancellationToken cancellationToken)
{
    // Posts with ID 42 must not be visible.
    var result = previousResult.WithFilter(source => source.Where(p => p.Id != 42));
    return base.HandleRequestAsync(result, cancellationToken);
}

public override Task<ServiceResponse<PostWithAuthorDto>> HandleResponseAsync(ServiceResponse<PostWithAuthorDto> previousResponse, CancellationToken cancellationToken)
{
    // Hide the author for some posts.
    if (previousResponse.Succeeded && previousResponse.ResponseObject.AuthorId % 2 == 0)
        previousResponse.ResponseObject.Author = null;

    return base.HandleResponseAsync(previousResponse, cancellationToken);
}
```

The service must call the handlers itself, see [Custom Services](services.md#writing-your-own-service-methods).

### Requiring an authenticated user

The `UserIsAuthorized...AuthorizationHandler` classes return `401 Unauthorized` for every request of an unauthenticated user. They also give you virtual `...WithUserAsync` methods (for example `HandleDeleteRequestWithUserAsync`, `HandleGetListRequestWithUserAsync`, `HandleGetSingleResponseWithUserAsync`) that receive the current `ClaimsPrincipal user`. Override `GetUser()` to change how the user is determined.

```csharp
public class PostAuthorizationHandler : UserIsAuthorizedCrudAuthorizationHandler<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    public PostAuthorizationHandler(IUserAccessor userAccessor) : base(userAccessor) { }

    protected override Task<AuthorizationResult<Post, long, byte[]>> HandleDeleteRequestWithUserAsync(
        AuthorizationResult<Post, long, byte[]> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
        => Task.FromResult(user.IsInRole("Admin") ? previousResult : previousResult.WithStatus(HttpStatusCode.Forbidden));
}
```

### IUserAccessor

`IUserAccessor` (`RESTworld.Business.Authorization.Abstractions`) exposes the current `ClaimsPrincipal? User`. RESTworld registers an implementation that reads it from the `HttpContext` when you call `AddRestWorld()`, so you can integrate any authentication mechanism (JWT, cookies, external providers) with normal ASP.NET Core means and inject `IUserAccessor` into handlers and services.

### Results

- Create results with `AuthorizationResult.Ok`, `Forbidden`, `Unauthorized` or `FromStatus<TEntity, T1[, T2]>(...)`. Services without database access use `AuthorizationResultWithoutDb`.
- `WithFilter(source => ...)` adds a filter on the entity query (multiple filters are combined). `WithStatus(HttpStatusCode)` changes the status. `ChangeParameter`/`ChangeParameters` replace the request values.

## Registering handlers

Use the `IHostApplicationBuilder` extension methods in `Program.cs`. For every method there is an `IServiceCollection` variant that additionally takes the `IConfiguration` as first argument; it is used to honour the `DisableAuthorization` setting.

```csharp
// Pipeline and handler together (ApiVersion is optional)
builder.AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, MyAuthorizationHandler>();
builder.AddReadPipelineWithAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, MyReadAuthorizationHandler>();

// Pipeline with custom service and handler (as in the ExampleBlog)
builder.AddCrudPipelineWithCustomServiceAndAuthorization<BlogDatabase, Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto, PostService, BlogpostAuthorizationHandler>();
builder.AddReadPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService, MyReadAuthorizationHandler>();

// Handler only, for a pipeline that is already registered
builder.AddCrudAuthorizationHandler<MyAuthorizationHandler, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>();
builder.AddReadAuthorizationHandler<MyReadAuthorizationHandler, TEntity, TQueryDto, TGetListDto, TGetFullDto>();
builder.AddBasicAuthorizationHandler<MyBasicHandler, TRequest, TResponse>();
builder.AddBasicAuthorizationHandler<MyBasicHandler, TEntity, TRequest, TResponse>();

// Custom service plus basic handler (you write the controller yourself)
builder.AddCustomServiceAndAuthorization<TRequest, TResponse, TService, MyBasicHandler>();
builder.AddCustomServiceAndAuthorization<TEntity, TRequest, TResponse, TService, MyBasicHandler>();
```

A generic handler that is shared by several pipelines is registered by closing it with the DTO types of each pipeline:

```csharp
builder.AddCrudAuthorizationHandler<
    UserIsAuthorizedCrudAuthorizationHandler<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>,
    Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>();
```

Handlers are registered as scoped services (as their interface and as their own type). The ExampleBlog's custom service injects the concrete handler type, so it registers service and handler directly:

```csharp
services.AddScoped<MyCustomService>();
services.AddScoped<MyCustomAuthorizationHandler>();
```

Handlers added through the `Add...AuthorizationHandler`, `Add...WithAuthorization` and `AddCustomServiceAndAuthorization` methods are skipped when `RESTworld:DisableAuthorization` is `true` (the ExampleBlog sets this for local development). Never set it in production, see [Configuration](configuration.md).

## Versioned APIs

Handlers are resolved by their generic arguments, so a pipeline version with its own DTO types needs its own handler registration:

```csharp
builder.AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDtoV1, TQueryDtoV1, TGetListDtoV1, TGetFullDtoV1, TUpdateDtoV1, MyAuthorizationHandlerV1>(new ApiVersion(1, 0), true);
builder.AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, MyAuthorizationHandler>(new ApiVersion(2, 0));
```

A generic handler can be reused for all versions by closing it with each version's DTOs. For custom services and controllers write one handler per DTO version, like the ExampleBlog's `MyCustomAuthorizationHandler` (`PostWithAuthorDto`) and `MyCustomAuthorizationHandlerV1` (`PostWithAuthorDtoV1`), and register both with `AddScoped`. See [Mapping and Versioning](mapping-and-versioning.md).

Pair this guide with [Pipeline Overview](pipeline-overview.md) to decide where the handler runs, [Custom Services](services.md) for services that call handlers themselves, and [Validation](validation.md) for rules that depend on the payload. Review [Configuration](configuration.md) if you need to disable authorization temporarily in development.

## Best practices

- Keep authorization logic free of side effects. Manipulate the request or the response only to filter, deny or hide data for specific users or groups.
- Centralize business-rule checks in handlers to keep controllers and services focused.
- Put all response changes of a pipeline into one handler: each response handler is called with the original service response and only the result of the last one is used.
- Prefer filters (`WithFilter`) over loading and checking entities in the response handler; filters are applied in the database query.

For client-side considerations, continue with [Angular Client Development](client-development-angular.md) or [.NET Client Development](client-development-dotnet.md), or explore deployment readiness in [Health and Operations](health-and-operations.md).
