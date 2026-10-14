---
name: restworld-authorization-handler
description: 'Write and register a RESTworld authorization handler. Use when asked to secure a RESTworld endpoint, restrict which rows a user may see, deny requests with 401/403, change or hide DTO properties per user, require an authenticated user, implement ICrudAuthorizationHandler, IReadAuthorizationHandler or IBasicAuthorizationHandler, derive from CrudAuthorizationHandlerBase, ReadAuthorizationHandlerBase, BasicAuthorizationHandlerBase, UserIsAuthorizedCrudAuthorizationHandler, UserIsAuthorizedReadAuthorizationHandler or UserIsAuthorizedBasicAuthorizationHandler, use IUserAccessor, filter with AuthorizationResult.WithFilter / WithStatus, or register handlers with AddCrudAuthorizationHandler, AddReadAuthorizationHandler, AddBasicAuthorizationHandler, AddCrudPipelineWithAuthorization, AddReadPipelineWithAuthorization, AddCrudPipelineWithCustomServiceAndAuthorization or AddCustomServiceAndAuthorization. Also covers versioned handlers and the DisableAuthorization setting.'
argument-hint: 'Describe who may do what (e.g. "users only see their own posts", "only admins may delete"). Optionally provide entity/DTO type names, whether the endpoint uses a pipeline or a custom service/controller, and the API version.'
---

# RESTworld: Authorization Handler

## When to Use
- Restrict access to a RESTworld pipeline or custom service (authentication required, roles, row-level filtering, per-user DTO changes).
- Add the authorization part to a new endpoint, or fix its registration.

## How it works
Services call every registered handler **before** the business logic (`Handle...RequestAsync`) and **after** it (`Handle...ResponseAsync`).
- A request handler receives the previous `AuthorizationResult<...>` and returns it, modified or not. If the returned `Status` is not `200 OK`, the call stops and the status (e.g. 401, 403) is sent to the client.
- The `Filter` on the result is applied to the `IQueryable<TEntity>` used for reading and for update/delete, so filtered-out rows behave as if they did not exist (404 on single get/update/delete).
- A response handler receives the `ServiceResponse<T>` and returns it, a changed DTO, or a different response.
- With no handler registered, no authorization is performed (a warning is logged once).
- Request handlers are chained in registration order (each gets the previous result). Every response handler is called with the original service response and only the last handler's return value is used, so put response changes into **one** handler per pipeline.

## Choose the base class (namespace `RESTworld.Business.Authorization`)

| Used with | Interface | Base class (all methods `virtual`, default = pass through) | With authenticated-user check |
|---|---|---|---|
| Crud pipeline | `ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>` | `CrudAuthorizationHandlerBase<...>` | `UserIsAuthorizedCrudAuthorizationHandler<...>` |
| Read pipeline | `IReadAuthorizationHandler<TEntity, TQueryDto, TGetListDto, TGetFullDto>` | `ReadAuthorizationHandlerBase<...>` | `UserIsAuthorizedReadAuthorizationHandler<...>` |
| Custom service | `IBasicAuthorizationHandler<TRequest, TResponse>` | `BasicAuthorizationHandlerBase<TRequest, TResponse>` | `UserIsAuthorizedBasicAuthorizationHandler<TRequest, TResponse>` |
| Custom service with DB filter | `IBasicAuthorizationHandler<TEntity, TRequest, TResponse>` | `BasicAuthorizationHandlerBase<TEntity, TRequest, TResponse>` | `UserIsAuthorizedBasicAuthorizationHandler<TEntity, TRequest, TResponse>` |

(`IBasicAuthorizationHandler<TResponse>` without a request parameter also exists but has no base class.) A Crud handler is also used for read operations because `ICrudAuthorizationHandler` extends `IReadAuthorizationHandler`. `HandleGetHistory...Async` on `ReadAuthorizationHandlerBase` is not virtual (pass-through).

### Crud/Read methods
`HandleCreateRequestAsync` / `HandleCreateResponseAsync` (single and `IReadOnlyCollection<TCreateDto>` overloads), `HandleGetListRequestAsync` / `...ResponseAsync`, `HandleGetSingleRequestAsync` / `...ResponseAsync`, `HandleUpdateRequestAsync` / `...ResponseAsync` (single and `IUpdateMultipleRequest<TUpdateDto, TEntity>`), `HandleDeleteRequestAsync` / `...ResponseAsync`. Request parameters are `AuthorizationResult<TEntity, T>` where `T` is `TCreateDto`, `IGetListRequest<TEntity, TQueryDto, TGetListDto>`, `long` (get single), `TUpdateDto` or `long, byte[]` (delete). Basic handlers have `HandleRequestAsync` / `HandleResponseAsync`.

## Typical handlers

**Filter and deny (ExampleBlog `BlogpostAuthorizationHandler : CrudAuthorizationHandlerBase<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>`):**
```csharp
public override Task<AuthorizationResult<Post, IGetListRequest<Post, PostQueryDto, PostListDto>>> HandleGetListRequestAsync(
    AuthorizationResult<Post, IGetListRequest<Post, PostQueryDto, PostListDto>> previousResult, CancellationToken cancellationToken)
    => Task.FromResult(previousResult.WithFilter(source => source.Where(p => p.Id % 2 == 0)));   // only some rows

public override Task<AuthorizationResult<Post, long>> HandleGetSingleRequestAsync(
    AuthorizationResult<Post, long> previousResult, CancellationToken cancellationToken)
    => Task.FromResult(previousResult.Value1 == 13
        ? AuthorizationResult.FromStatus<Post, long>(HttpStatusCode.Forbidden, previousResult.Value1)   // or previousResult.WithStatus(...)
        : previousResult);

public override Task<ServiceResponse<PostGetFullDto>> HandleGetSingleResponseAsync(
    ServiceResponse<PostGetFullDto> previousResponse, CancellationToken cancellationToken)
    => Task.FromResult(previousResponse.ResponseObject?.Id == 42
        ? ServiceResponse.FromProblem<PostGetFullDto>((HttpStatusCode)42, "This is the answer!")
        : previousResponse);
```

**Basic handler for a custom service (`MyCustomAuthorizationHandler : BasicAuthorizationHandlerBase<Post, long, PostWithAuthorDto>`):**
```csharp
public override Task<AuthorizationResult<Post, long>> HandleRequestAsync(AuthorizationResult<Post, long> previousResult, CancellationToken cancellationToken)
    => base.HandleRequestAsync(previousResult.WithFilter(source => source.Where(p => p.Id != 42)), cancellationToken);

public override Task<ServiceResponse<PostWithAuthorDto>> HandleResponseAsync(ServiceResponse<PostWithAuthorDto> previousResponse, CancellationToken cancellationToken)
{
    if (previousResponse.Succeeded && previousResponse.ResponseObject.AuthorId % 2 == 0)
        previousResponse.ResponseObject.Author = null;      // hide data per user
    return base.HandleResponseAsync(previousResponse, cancellationToken);
}
```

**Real user checks.** Derive from `UserIs*AuthorizationHandler<...>`: unauthenticated users get 401, and you override the `Handle...WithUserAsync` methods that additionally receive the `ClaimsPrincipal user`:
```csharp
public class MyPostHandler : UserIsAuthorizedCrudAuthorizationHandler<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>
{
    public MyPostHandler(IUserAccessor userAccessor) : base(userAccessor) { }

    protected override Task<AuthorizationResult<Post, long, byte[]>> HandleDeleteRequestWithUserAsync(
        AuthorizationResult<Post, long, byte[]> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
        => Task.FromResult(user.IsInRole("Admin") ? previousResult : previousResult.WithStatus(HttpStatusCode.Forbidden));

    protected override Task<AuthorizationResult<Post, IGetListRequest<Post, PostQueryDto, PostListDto>>> HandleGetListRequestWithUserAsync(
        AuthorizationResult<Post, IGetListRequest<Post, PostQueryDto, PostListDto>> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var name = user.Identity!.Name;
        return Task.FromResult(previousResult.WithFilter(source => source.Where(p => p.CreatedBy == name)));
    }
}
```
Other `...WithUser` hooks follow the names above (e.g. `HandleCreateResponseWithUserAsync`, `HandleUpdateRequestWithUserAsync`); override `GetUser()` to change how the user is found.

## IUserAccessor
`RESTworld.Business.Authorization.Abstractions.IUserAccessor` exposes `ClaimsPrincipal? User` from the current `HttpContext`. It is registered for you by `AddRestWorld()` (`AddUserAccessor()`); inject it into handlers and services. Authentication (JWT, cookies, ...) is configured with normal ASP.NET Core means.

## Results helpers
- `AuthorizationResult.Ok/Forbidden/Unauthorized/FromStatus<TEntity[, T1[, T2]]>(...)`, and `AuthorizationResultWithoutDb.Ok/Forbidden/Unauthorized/FromStatus(...)` for services without DB.
- Extension methods: `WithFilter(Func<IQueryable<TEntity>, IQueryable<TEntity>>)` (filters are combined), `WithStatus(HttpStatusCode)`; `ChangeParameter(s)` on the instance.

## Registration
Use the `IHostApplicationBuilder` extensions (`using Microsoft.Extensions.DependencyInjection;`) in `Program.cs`. `IServiceCollection` variants exist with an additional `IConfiguration configuration` first parameter (needed to honour `DisableAuthorization`).

| Goal | Call |
|---|---|
| Crud pipeline + handler | `builder.AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, THandler>(ApiVersion? apiVersion = null, bool isDeprecated = false)` |
| Read pipeline + handler | `builder.AddReadPipelineWithAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, THandler>(apiVersion, isDeprecated)` |
| Custom-service pipeline + handler | `builder.AddCrudPipelineWithCustomServiceAndAuthorization<..., TService, THandler>()`, `builder.AddReadPipelineWithCustomServiceAndAuthorization<..., TService, THandler>()` (ExampleBlog: `PostService` + `BlogpostAuthorizationHandler`) |
| Handler only (pipeline already added) | `builder.AddCrudAuthorizationHandler<THandler, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>()`, `builder.AddReadAuthorizationHandler<THandler, TEntity, TQueryDto, TGetListDto, TGetFullDto>()` |
| Basic handler only | `builder.AddBasicAuthorizationHandler<THandler, TResponse>()`, `<THandler, TRequest, TResponse>()`, `<THandler, TEntity, TRequest, TResponse>()` |
| Service + basic handler | `builder.AddCustomServiceAndAuthorization<TRequest, TResponse, TService, THandler>()` and `<TEntity, TRequest, TResponse, TService, THandler>()` (you still write the controller) |
| Custom service/handler as concrete scoped types | `services.AddScoped<MyCustomService>(); services.AddScoped<MyCustomAuthorizationHandler>();` (as ExampleBlog does, because the service injects the concrete handler type) |

A shared generic handler is registered by closing it explicitly:
```csharp
builder.AddCrudAuthorizationHandler<
    UserIsAuthorizedCrudAuthorizationHandler<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>,
    Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>();
```
Handlers are registered scoped (the interface and the concrete type). When `RESTworld:DisableAuthorization` is `true` (ExampleBlog `appsettings.json`), handlers registered through the `Add...AuthorizationHandler` / `...WithAuthorization` methods are **not** registered. Never enable it in production.

## Versioned handlers
Pipeline handlers are resolved by their generic type arguments, so each API version with its own DTO types gets its own handler registration (`AddCrudPipelineWithAuthorization<..., AuthorDtoV1, ..., THandler>(new ApiVersion(1, 0), true)`). For custom services/controllers create one handler per version, like `MyCustomAuthorizationHandler` (V2 DTOs) and `MyCustomAuthorizationHandlerV1` (`BasicAuthorizationHandlerBase<Post, long, PostWithAuthorDtoV1>`), and register both. See [restworld-add-api-version](../restworld-add-api-version/SKILL.md).

## Verification checklist
- [ ] Handler base class matches the pipeline type (Crud / Read / Basic) and all generic arguments match the registration.
- [ ] Request handlers return `previousResult` (modified) instead of throwing; denials use `WithStatus` with 401/403.
- [ ] Handler registered with one of the methods above (or `AddScoped` for concrete custom-service handlers).
- [ ] `DisableAuthorization` is not `true` where authorization is required.
- [ ] Test an authorized, an unauthorized, and a filtered request.

## Reference
- [Authorization](https://github.com/wertzui/RESTworld/blob/main/doc/authorization.md) · [Services](https://github.com/wertzui/RESTworld/blob/main/doc/services.md) · [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md) · [Pipeline Overview](https://github.com/wertzui/RESTworld/blob/main/doc/pipeline-overview.md)
- Related skills: [restworld-custom-service](../restworld-custom-service/SKILL.md), [restworld-custom-controller](../restworld-custom-controller/SKILL.md), [restworld-choose-pipeline](../restworld-choose-pipeline/SKILL.md)
