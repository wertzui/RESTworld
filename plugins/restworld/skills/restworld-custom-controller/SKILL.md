---
name: restworld-custom-controller
description: 'Write a custom RESTworld controller. Use when asked to add an endpoint that does not fit the CRUD/read pipeline (extra routes, aggregated or joined data, non-DB resources, file/image downloads, actions), derive from RestControllerBase or HalControllerBase, build HAL responses with IODataResourceFactory and IResultFactory (CreateOkResultBasedOnOutcomeAsync, CreateCreatedResultBasedOnOutcomeAsync, CreatePagedCollectionResultBasedOnOutcomeAsync, CreateEmptyResultBasedOnOutcome), cache service results with ICacheHelper and CachingOptions (Get, GetList, GetHistory), return errors with IErrorResultFactory, set routes, [ApiVersion], [ProducesResponseType] and ApiConventionMethod, or understand that controllers are discovered automatically and only their service/handler must be registered. Based on MyCustomController and PhotoController of ExampleBlog.'
argument-hint: 'Describe the endpoint (route, HTTP verb, input, output DTO, whether it needs the database/HAL, API version). Optionally name the service it should call.'
---

# RESTworld: Custom Controller

## When to Use
- The endpoint is not a plain CRUD/read of one entity: aggregated/joined data, extra routes next to the standard ones, actions, non-database resources, binary downloads.
- First check [restworld-choose-pipeline](../restworld-choose-pipeline/SKILL.md). Only write a controller if a pipeline (even with a custom service) cannot express the endpoint.

## Pick the base class

| Response | Derive from |
|---|---|
| A HAL resource (links, HAL-Forms templates, caching, versioning) | `RESTworld.AspNetCore.Controller.RestControllerBase` |
| HAL response without RESTworld helpers | `HAL.AspNetCore.Controllers.HalControllerBase` |
| Plain HTTP (files, images, no HAL) | `Microsoft.AspNetCore.Mvc.ControllerBase` with `[ApiController]` |
| Standard endpoints plus extras for an entity | derive from `CrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>` or `ReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto>` and register with `RestControllerFeatureProvider.AddCustomCrudController<..., TController>()` / `AddCustomReadController<..., TController>()` (`RESTworld.AspNetCore.Controller`) *before* the host is built, instead of `Add...Pipeline` |

`RestControllerBase` gives you the protected `ResourceFactory` (`IODataResourceFactory`) and `Cache` (`ICacheHelper`), a default `[Route("[controller]")]`, and default `ProducesResponseType` for 401, 403 and 503 and `ProducesErrorResponseType(typeof(Resource<ProblemDetails>))`.

## Key rules
- **Controllers are discovered automatically** (any controller class in the application assembly, e.g. a `Controllers` folder). Do **not** register controllers; only register the service and authorization handler they use (`services.AddScoped<MyCustomService>()`, `services.AddScoped<IPhotoService, PhotoService>()`).
- Keep the controller thin: call a service (see [restworld-custom-service](../restworld-custom-service/SKILL.md)), let `IResultFactory` convert the `ServiceResponse<T>` into success or error result.
- Always declare responses: `[ProducesResponseType(typeof(Resource<TDto>), StatusCodes.Status200OK)]`, `[ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]`, plus `[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]` and `[Description("...")]` for OpenAPI.
- Set an explicit route (`[Route("postwithauthor")]`) and, if the API is versioned, `[ApiVersion("2")]` (older: `[ApiVersion("1", Deprecated = true)]`). Use `[ApiVersionNeutral]` for version-independent endpoints. Versions with different DTOs need separate controller classes with the same route (`MyCustomController` / `MyCustomController1`).

## Example: HAL endpoint (ExampleBlog `MyCustomController`)
```csharp
[Route("postwithauthor")]
[ApiVersion("2")]
public class MyCustomController : RestControllerBase
{
    private readonly MyCustomService _service;
    private readonly IResultFactory _resultFactory;

    public MyCustomController(MyCustomService service, IODataResourceFactory resourceFactory, IResultFactory resultFactory, ICacheHelper cache)
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
        [Description("The ID of the post")] long id, CancellationToken cancellationToken)
    {
        var response = await Cache.GetOrCreateWithCurrentUserAsync(
            nameof(GetPostWithAuthorAsync) + "_" + id, nameof(CachingOptions.Get),
            _ => _service.GetPostWithAuthorAsync(id, cancellationToken));

        return await _resultFactory.CreateOkResultBasedOnOutcomeAsync<PostWithAuthorDto, PostWithAuthorDto>(
            response, action: ActionHelper.StripAsyncSuffix(nameof(GetPostWithAuthorAsync)));
    }
}
```
Namespaces: `HAL.AspNetCore.OData.Abstractions` (`IODataResourceFactory`), `HAL.AspNetCore.Utils` (`ActionHelper`), `HAL.Common` (`Resource<T>`), `RESTworld.AspNetCore.Caching` (`ICacheHelper`), `RESTworld.AspNetCore.DependencyInjection` (`CachingOptions`), `RESTworld.AspNetCore.Results.Abstractions` (`IResultFactory`), `Asp.Versioning`.
The `action` argument is the **action name without `Async`**; it is used to generate the self link and the HAL-Forms template target. Pass `controller:` / `routeValues:` if the defaults (`id` of the DTO) do not match your route.

## IResultFactory (all return `ValueTask`; failed `ServiceResponse`s become an error result automatically)

| Method | Use |
|---|---|
| `CreateOkResultBasedOnOutcomeAsync<TValue, TTemplate>(ServiceResponse<TValue>, bool readOnly = true, string action = "Get", string? controller = null, object? routeValues = null)` | 200 with a single HAL resource |
| `CreateCreatedResultBasedOnOutcomeAsync<TValue, TTemplate>(..., bool readOnly = false, string getAction = "Get", ...)` | 201 + `Location` |
| `CreateOkCollectionResultBasedOnOutcomeAsync<TDto>(ServiceResponse<IReadOnlyCollection<TDto>>, ...)` / `CreateCreatedCollectionResultBasedOnOutcomeAsync<TDto>(...)` | Collections after bulk update/create |
| `CreatePagedCollectionResultBasedOnOutcomeAsync<TQueryDto, TDto>(ODataQueryOptions<TQueryDto>, ServiceResponse<IReadOnlyPagedCollection<TDto>>, ...)` | Paged list with OData links (also an overload with `ODataRawQueryOptions options, int? maxTop`) |
| `CreateEmptyResultBasedOnOutcome(ServiceResponse<object>, string action = "Delete", ...)` | 204 / error |
| `CreateResourceAsync<TValue, TTemplate>(TValue dto, HttpMethod method, bool readOnly = true, ...)`, `CreateCollectionResourceAsync`, `CreatePagedCollectionResourceAsync` | Build resources manually |
| `MakeFormReadOnly`, `MakeTemplateReadOnly`, `MakeTemplatesReadOnly` | Make HAL-Forms read-only |

`TTemplate` is the DTO used for the HAL-Forms template (often the same as `TValue`; for create/edit forms use the create/update DTO).
Custom errors: inject `RESTworld.AspNetCore.Results.Errors.Abstractions.IErrorResultFactory` and return `CreateError(StatusCodes.Status400BadRequest, "message", action: "Post")` (also overloads for `ProblemDetails` and `ServiceResponse<T>`).

## Caching
`Cache` (`ICacheHelper`) is an in-memory cache with timeouts from `RESTworld:Caching` (`CachingOptions`, a `Dictionary<string, TimeSpan>`; defaults `Get`, `GetList`, `GetHistory` = 10 s).
- `GetOrCreateWithCurrentUserAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory)` – key is prefixed with the current user's name, so users never see each other's data. `...WithoutUserAsync` shares the entry between users; `...WithUserAsync(key, username, ...)` for an explicit user. Sync variants without `Async` exist.
- `timeoutKey` must exist in `CachingOptions` (`nameof(CachingOptions.Get)`), otherwise an `ArgumentException` is thrown. Additional keys can be added to the dictionary through configuration.
- Invalidate: `RemoveWithCurrentUser(key)`, `RemoveWithoutUser(key)`, `RemoveWithUser(key, username)`, `RemoveForAllUsers(key)`; list helpers `RemoveAllListEntriesWith...`.
- Extension helpers (`CacheHelperExtensions`) build standard keys: `Cache.CacheGetWithCurrentUserAsync(id, factory)`, `CacheGetListWithCurrentUserAsync(options.RawValues, factory)`, `RemoveGetForAllUsers<ServiceResponse<TDto>>(id)`, `RemoveGetListForAllUsers<ServiceResponse<TDto>>()`. In custom controllers an explicit key (as above) is simplest; **remove the entries after writes** that affect them.
- Cache only `ServiceResponse<T>` results from the service (the factory returns the response; failures are cached as well for the timeout).

## Plain controller (ExampleBlog `PhotoController`)
```csharp
[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _service;
    public PhotoController(IPhotoService service) => _service = service;

    [HttpGet("{email}")]
    public async Task<FileResult?> GetAsync(string email, CancellationToken cancellationToken)
    {
        var response = await _service.GetPhotoAsync(email, cancellationToken);
        return response.Succeeded ? File(response.ResponseObject, "image/*") : File(Array.Empty<byte>(), "image/*");
    }
}
```
Register only the service: `services.AddHttpClient(); services.AddScoped<IPhotoService, PhotoService>();`.

## Steps
1. Create the service (and DTOs/mapper if database-backed) → [restworld-custom-service](../restworld-custom-service/SKILL.md), [restworld-create-dtos](../restworld-create-dtos/SKILL.md).
2. Create the controller in the web project's `Controllers` folder with route, version, response attributes and the calls above.
3. Register service (`AddScoped`), authorization handler ([restworld-authorization-handler](../restworld-authorization-handler/SKILL.md)) and read mappers (`AddReadMapper` with a Mapperly mapper; ExampleBlog still uses the legacy `AddReadAutoMapper` here) in `Program.cs`.
4. Call the endpoint; check the HAL links and the OpenAPI document.

## Verification checklist
- [ ] Controller derives from `RestControllerBase` (HAL) or `ControllerBase` (plain) and is *not* registered manually.
- [ ] Service, handler and mappers are registered; the service result is passed through `IResultFactory`.
- [ ] `action` passed to the factory is the action name without `Async`.
- [ ] Route, `[ApiVersion]`, `ProducesResponseType` for success and 404/400 present.
- [ ] Cache key includes all parameters; the timeout key exists; writes invalidate affected keys.

## Reference
- [Custom Controllers](https://github.com/wertzui/RESTworld/blob/main/doc/custom-controllers.md) · [Services](https://github.com/wertzui/RESTworld/blob/main/doc/services.md) · [Authorization](https://github.com/wertzui/RESTworld/blob/main/doc/authorization.md) · [Pipeline Overview](https://github.com/wertzui/RESTworld/blob/main/doc/pipeline-overview.md) · [Configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md)
- Related skills: [restworld-add-api-version](../restworld-add-api-version/SKILL.md), [restworld-api-extras](../restworld-api-extras/SKILL.md), [restworld-halforms-customization](../restworld-halforms-customization/SKILL.md)
