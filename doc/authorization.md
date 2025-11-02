# Authorization

RESTworld ships with extensibility points for resource-based authorization. Implement handlers to enforce access rules before and after business logic runs.

## Implementing handlers

Create a handler per pipeline or reuse a generic handler when possible.

```csharp
public class MyAuthorizationHandler : ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
{
    // Inject dependencies such as IUserAccessor, services, etc.

    public Task AuthorizeCreateAsync(...) { /* logic */ }
    public Task AuthorizeGetListAsync(...) { /* logic */ }
    public Task AuthorizeGetAsync(...) { /* logic */ }
    public Task AuthorizeUpdateAsync(...) { /* logic */ }
    public Task AuthorizeDeleteAsync(...) { /* logic */ }
}
```

Use the provided `IUserAccessor` to access the current principal. RESTworld populates it from `HttpContext`, letting you integrate any authentication mechanism (JWT, cookies, external providers) without duplicating functionality already available in the ASP.NET Core ecosystem.

## Registering handlers

Choose the registration pattern that matches your pipeline:

```csharp
// Concrete handler for a pipeline
services.AddAuthorizationHandler<MyAuthorizationHandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Generic handler shared across pipelines
services.AddAuthorizationHandler<MyGenericAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>,
    TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

// Pipeline + handler shortcuts
services.AddRestPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, MyAuthorizationHandler>();
services.AddRestPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto,
    MyGenericAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();

// Pipelines with custom services
services.AddRestPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto,
    TService, MyAuthorizationHandler>();
services.AddRestPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto,
    TService, MyGenericAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
```

Pair this guide with [Pipeline Overview](pipeline-overview.md) to decide where the handler runs, and review [Configuration](configuration.md) if you need to disable authorization temporarily in development.

## Best practices

- Keep authorization logic side-effect free. Manipulate the request/response when implementing filtering for specific users/groups.
- Centralize business-rule checks in handlers to keep controllers and services focused.

For client-side considerations, continue with [Angular Client Development](client-development-angular.md) or [.NET Client Development](client-development-dotnet.md), or explore deployment readiness in [Health and Operations](health-and-operations.md).
