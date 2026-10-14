# Custom Controllers

The built-in pipelines expose the standard endpoints for an entity (get, list, create, update, delete). Write a custom controller when an endpoint does not fit that shape: aggregated or joined data, extra routes, actions, resources that are not stored in the database, or binary downloads. Custom controllers keep full HAL support when you derive from `RestControllerBase`.

Use [Choosing a Pipeline](choosing_a_pipeline.md) first; a [custom service](services.md) behind a normal pipeline is often enough.

## Base classes

| Response | Derive from |
|---|---|
| HAL resource with links, HAL-Forms, caching and versioning | `RestControllerBase` (`RESTworld.AspNetCore.Controller`) |
| HAL response without the RESTworld helpers | `HalControllerBase` (`HAL.AspNetCore.Controllers`) |
| Plain HTTP such as files or images | `ControllerBase` with `[ApiController]` |
| The standard endpoints plus your own | `CrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>` or `ReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto>` |

`RestControllerBase` provides:

- `ResourceFactory` (`IODataResourceFactory`) – creates HAL resources with links to controller actions.
- `Cache` (`ICacheHelper`) – cache for service results, see [Caching](#caching).
- `[Route("[controller]")]` as default route and `ProducesResponseType` attributes for `401`, `403` and `503` with a `Resource<ProblemDetails>` body.

Its constructor takes `IODataResourceFactory resourceFactory, ICacheHelper cache`.

## Controllers are discovered automatically

ASP.NET Core finds every controller class in your application. You **do not register controllers**. Register only what they use:

```csharp
// ExampleBlog Program.cs
services.AddScoped<MyCustomService>();                 // service for MyCustomController
services.AddScoped<MyCustomAuthorizationHandler>();    // its authorization handler
rwBuilder.AddReadAutoMapper<Post, PostWithAuthorDto, PostWithAuthorDto, PostWithAuthorDto>();   // legacy AutoMapper registration; new code: AddReadMapper<..., TMapper>() with a Mapperly mapper

services.AddHttpClient();
services.AddScoped<IPhotoService, PhotoService>();     // service for PhotoController, no controller registration
```

To replace the generic controller of a pipeline with your own subclass of `CrudController`/`ReadController`, register it with `RestControllerFeatureProvider.AddCustomCrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TController>()` or `AddCustomReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto, TController>()`. A controller with the same name that already exists in the application wins over the generic one.

## A HAL controller

`MyCustomController` returns a post together with its author on `/postwithauthor/{id}`:

```csharp
[Route("postwithauthor")]
[ApiVersion("2")]
public class MyCustomController : RestControllerBase
{
    private readonly MyCustomService _service;
    private readonly IResultFactory _resultFactory;

    public MyCustomController(
        MyCustomService service,
        IODataResourceFactory resourceFactory,
        IResultFactory resultFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
    }

    [HttpGet("{id:long}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(typeof(Resource<PostWithAuthorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
    [Description("Gets a blog post including the author information.")]
    public async Task<ActionResult<Resource<PostWithAuthorDto>>> GetPostWithAuthorAsync(
        [Description("The ID of the post")] long id,
        CancellationToken cancellationToken)
    {
        var response = await Cache.GetOrCreateWithCurrentUserAsync(
            nameof(GetPostWithAuthorAsync) + "_" + id,
            nameof(CachingOptions.Get),
            _ => _service.GetPostWithAuthorAsync(id, cancellationToken));

        var result = await _resultFactory.CreateOkResultBasedOnOutcomeAsync<PostWithAuthorDto, PostWithAuthorDto>(
            response,
            action: ActionHelper.StripAsyncSuffix(nameof(GetPostWithAuthorAsync)));

        return result;
    }
}
```

The needed namespaces are `Asp.Versioning`, `HAL.AspNetCore.OData.Abstractions`, `HAL.AspNetCore.Utils`, `HAL.Common`, `RESTworld.AspNetCore.Caching`, `RESTworld.AspNetCore.Controller`, `RESTworld.AspNetCore.DependencyInjection` and `RESTworld.AspNetCore.Results.Abstractions`.

The service returns a `ServiceResponse<PostWithAuthorDto>`; the controller only caches it and hands it to the result factory. See [Custom Services](services.md) for the service part.

## A plain controller

When no HAL is needed, write a normal ASP.NET Core controller. `PhotoController` returns an image for the list views of the frontend:

```csharp
[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _service;

    public PhotoController(IPhotoService service)
    {
        _service = service;
    }

    [HttpGet("{email}")]
    public async Task<FileResult?> GetAsync(string email, CancellationToken cancellationToken)
    {
        var response = await _service.GetPhotoAsync(email, cancellationToken);

        if (response.Succeeded)
            return File(response.ResponseObject, "image/*");

        return File(Array.Empty<byte>(), "image/*");
    }
}
```

## Routes, versions and response attributes

- Set the route explicitly: `[Route("postwithauthor")]` (or `[Route("[controller]")]`).
- Use `[ApiVersion("2")]` for the version the controller serves. Mark old versions as deprecated with `[ApiVersion("1", Deprecated = true)]`. Use `[ApiVersionNeutral]` for endpoints that do not depend on the version. For different DTOs per version write one controller class per version with the same route, as `MyCustomController` and `MyCustomController1` do. See [Mapping and Versioning](mapping-and-versioning.md).
- Declare what the endpoint returns: `[ProducesResponseType(typeof(Resource<TDto>), StatusCodes.Status200OK)]` for success and `[ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]` (and `400`/`409` where applicable) for errors. `[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]` and `[Description("...")]` improve the generated OpenAPI document.

## Building results with IResultFactory

`IResultFactory` (`RESTworld.AspNetCore.Results.Abstractions`) converts a `ServiceResponse<T>` into an `ObjectResult`. A failed response is turned into the matching error result (problem details or validation errors) automatically.

| Method | Result |
|---|---|
| `CreateOkResultBasedOnOutcomeAsync<TValue, TTemplate>(serviceResponse, readOnly = true, action = "Get", controller = null, routeValues = null)` | `200` with a single HAL resource |
| `CreateCreatedResultBasedOnOutcomeAsync<TValue, TTemplate>(serviceResponse, readOnly = false, getAction = "Get", controller = null, routeValues = null)` | `201` with a `Location` |
| `CreateOkCollectionResultBasedOnOutcomeAsync<TDto>(...)` and `CreateCreatedCollectionResultBasedOnOutcomeAsync<TDto>(...)` | Collections after bulk operations |
| `CreatePagedCollectionResultBasedOnOutcomeAsync<TQueryDto, TDto>(ODataQueryOptions<TQueryDto>, serviceResponse, ...)` | Paged list with OData links; an overload takes `ODataRawQueryOptions` and `maxTop` |
| `CreateEmptyResultBasedOnOutcome(serviceResponse, action = "Delete", ...)` | `204 No Content` or an error |
| `CreateResourceAsync<TValue, TTemplate>(dto, HttpMethod, ...)`, `CreateCollectionResourceAsync<TDto>(...)`, `CreatePagedCollectionResourceAsync(...)` | Build a resource without a service response |
| `MakeFormReadOnly`, `MakeTemplateReadOnly`, `MakeTemplatesReadOnly` | Make HAL-Forms read-only |

`TTemplate` is the DTO type used to generate the HAL-Forms template. `action` is the name of the controller action **without** the `Async` suffix (`ActionHelper.StripAsyncSuffix(nameof(...))`); it is used for the self link. Pass `controller` and `routeValues` when the defaults (`id` of the DTO) do not match your route.

For your own errors inject `IErrorResultFactory` (`RESTworld.AspNetCore.Results.Errors.Abstractions`) and return `CreateError(StatusCodes.Status400BadRequest, "message", "Post")`. There are overloads for a `ServiceResponse<T>` and for any `ProblemDetails`.

## Caching

`ICacheHelper` (`RESTworld.AspNetCore.Caching`) caches service results in memory. The standard controllers use it for get, list and history. Timeouts are configured in `appsettings.json` under `RESTworld:Caching` (`CachingOptions`, a dictionary of `TimeSpan`s). The defaults for `Get`, `GetList` and `GetHistory` are 10 seconds.

```json
"RESTworld": {
  "Caching": {
    "Get": "00:00:30",
    "GetList": "00:00:05",
    "GetHistory": "00:00:10"
  }
}
```

| Method | Description |
|---|---|
| `GetOrCreateWithCurrentUserAsync<TItem>(key, timeoutKey, factory)` | Cache entry per current user (the user name is added to the key). Use for anything that depends on authorization. |
| `GetOrCreateWithoutUserAsync<TItem>(key, timeoutKey, factory)` | One entry shared by all users. |
| `GetOrCreateWithUserAsync<TItem>(key, username, timeoutKey, factory)` | Entry for an explicit user. |
| `SetWithCurrentUser`, `SetWithoutUser`, `SetWithUser` | Store a value directly. |
| `RemoveWithCurrentUser(key)`, `RemoveWithoutUser(key)`, `RemoveWithUser(key, username)`, `RemoveForAllUsers(key)` | Invalidate entries. |
| `RemoveAllListEntriesWithCurrentUser`, `RemoveAllListEntriesWithoutUser`, `RemoveAllListEntriesForAllUsers` (`typeKey, action`) | Invalidate every list entry of a type regardless of the query options. |

Synchronous variants without `Async` exist for the `GetOrCreate` methods.

- `timeoutKey` must exist in `CachingOptions`, usually `nameof(CachingOptions.Get)`, `GetList` or `GetHistory`; any other key throws an `ArgumentException`.
- The key must contain every parameter that influences the result (`nameof(GetPostWithAuthorAsync) + "_" + id`).
- Cache the `ServiceResponse<T>` the service returns and pass it to `IResultFactory`.
- Remove affected entries after writes. Extension methods in `CacheHelperExtensions` create the standard keys used by the built-in controllers, for example `Cache.CacheGetWithCurrentUserAsync(id, factory)`, `Cache.CacheGetListWithCurrentUserAsync(options.RawValues, factory)`, `Cache.RemoveGetForAllUsers<ServiceResponse<TDto>>(id)` and `Cache.RemoveGetListForAllUsers<ServiceResponse<TDto>>()`.

## Related

- [Custom Services](services.md) – the business logic behind your controller
- [Authorization](authorization.md) – `BasicAuthorizationHandlerBase` for services behind custom controllers
- [Validation](validation.md) – errors returned by `IResultFactory`
- [Pipeline Overview](pipeline-overview.md) and [Choosing a Pipeline](choosing_a_pipeline.md)
- [Configuration](configuration.md) – `RESTworld` settings
