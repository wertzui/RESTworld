---
name: restworld-custom-service
description: 'Write a custom RESTworld service by deriving from CrudServiceBase, ReadServiceBase, DbServiceBase or ServiceBase. Use when asked to customize endpoint logic, choose a base class, decide between the On...ing / On...ed hooks (OnCreatingInternalAsync, OnUpdatingInternalAsync, OnDeletingAsync, OnCreatedInternalAsync, OnUpdatedInternalAsync, OnGotSingleInternalAsync) and overriding CreateInternalAsync / UpdateInternalAsync / GetSingleInternalAsync; populate DTO properties not stored in the database (images, files, statistics); change the DbSet used for reading or updating; write a fully custom service method with a public MyMethodAsync wrapping TryExecuteWithAuthorizationAsync or TryExecuteAsync and a private MyMethodInternalAsync; build a service for a custom controller; return ServiceResponse or AuthorizationResult; or register a service with AddCrudPipelineWithCustomService, AddReadPipelineWithCustomService, AddCustomServiceAndAuthorization or AddScoped. Also covers exception translators.'
argument-hint: 'Describe what the service must do differently from the default pipeline. Optionally provide the entity and DTO type names, whether data comes from the database, and whether a custom controller will call the service.'
---

# RESTworld: Custom Service

## When to Use
- The default `CrudPipeline` / `ReadPipeline` is not enough: you must fill DTO properties that are not in the database, run side effects when entities are created/updated/deleted, or change which rows are read or updated.
- You need a service for a custom controller (database-backed or not).
- You want to know which methods can be overridden and how to register the service.

Deciding *whether* you need a custom service is covered by [restworld-choose-pipeline](../restworld-choose-pipeline/SKILL.md).

## Choose the base class

| You need | Derive from | Registered with |
|---|---|---|
| Full CRUD endpoints with extra logic | `CrudServiceBase<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>` | `AddCrudPipelineWithCustomService<..., TService>()` |
| Read-only endpoints with extra logic | `ReadServiceBase<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto>` | `AddReadPipelineWithCustomService<..., TService>()` |
| Database access + authorization + exception handling for a custom controller | `DbServiceBase<TContext>` | `services.AddScoped<TService>()` |
| No database (HTTP calls, files, ...) | `ServiceBase` | `services.AddScoped<IMyService, MyService>()` |

**Decision:** changing a standard entity endpoint that writes → `CrudServiceBase`; one that only reads → `ReadServiceBase`; a new operation for a custom controller that uses the database → `DbServiceBase<TContext>`; no database → `ServiceBase`. Take the most specific base that fits. Extra methods of your own are also allowed in a `Crud`/`ReadServiceBase` service (both derive from `DbServiceBase`), written with the pattern in "Writing your own service methods".

All live in `RESTworld.Business.Services`. `TContext` derives from `DbContextBase`, entities from `EntityBase` (read) / `ConcurrentEntityBase` (CRUD).

## Key rules
- The public methods (`GetSingleAsync`, `CreateAsync`, ...) are **not** virtual. They run authorization, exception handling and validation around the `protected virtual ...InternalAsync` / `On...` methods. **Override those.**
- Prefer an `On...` hook over replacing a whole `...InternalAsync` method (see "Hooks or `...InternalAsync` override?"). Call `base` unless you really replace the behaviour.
- Do not throw for expected failures. Return `ServiceResponse.FromStatus<T>(...)`, `FromProblem<T>(...)` or `FromFailedValidation<T>(...)`.
- Use `_contextFactory` (an `IDbContextFactory<TContext>`), never a shared `DbContext`. Use `_contextFactory.Parallel().Set<T>()` for concurrent queries (as `MyCustomService` does).
- Services are registered **scoped**. Mappers are registered separately (`AddCrudMapper` / `AddReadMapper`, see [mapping-and-versioning.md](https://github.com/wertzui/RESTworld/blob/main/doc/mapping-and-versioning.md)).
- Protected helpers from `ServiceBase`: `_userAccessor`, `_logger`, `GetCurrentUsersName()`.

## Constructors
`ILogger<T>` is covariant, so `ILogger<MyService>` can be passed where the base expects `ILogger<CrudServiceBase<...>>`.

```csharp
// CrudServiceBase (PostService / TestService in ExampleBlog.Business)
public PostService(
    IDbContextFactory<BlogDatabase> contextFactory,
    ICrudMapper<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto> mapper,
    IEnumerable<ICrudAuthorizationHandler<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>> authorizationHandlers,
    IValidationService<PostCreateDto, PostUpdateDto, Post> validationService,   // may be null
    IUserAccessor userAccessor,
    ILogger<CrudServiceBase<BlogDatabase, Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>> logger)
    : base(contextFactory, mapper, authorizationHandlers, validationService, userAccessor, logger) { }

// ReadServiceBase (AuthorStatisticsService): contextFactory, IReadMapper<...> mapper,
//   IEnumerable<IReadAuthorizationHandler<...>> authorizationHandlers, IUserAccessor, ILogger<...>
// DbServiceBase (MyCustomService): IDbContextFactory<TContext>, IEnumerable<IExceptionTranslator>, IUserAccessor, ILogger
// ServiceBase   (PhotoService):    IUserAccessor, ILogger
```

## Override points

| Base | Method (all `protected virtual`) | Purpose |
|---|---|---|
| Read | `GetDbSetForReadingAsync()` | Replace the `IQueryable<TEntity>` used for all reads |
| Read | `GetDbSetForReadingWithAuthorizationAsync(AuthorizationResult<TEntity>)` | Change how the authorization filter is applied |
| Read | `GetListInternalAsync`, `GetHistoryInternalAsync`, `GetSingleInternalAsync`, `GetSingleDtoAsync` | Replace read logic |
| Read | `OnGotListInternalAsync`, `OnGotHistoryInternalAsync`, `OnGotSingleInternalAsync` | Post-process DTOs after mapping |
| Crud | `CreateInternalAsync` (single and `IReadOnlyCollection<TCreateDto>`), `UpdateInternalAsync` (single and `IUpdateMultipleRequest<TUpdateDto, TEntity>`), `DeleteInternalAsync`, `PatchInternalAsync(JsonPatchDocument<TCreateDto, TUpdateDto>, CancellationToken)` | Replace write logic |
| Crud | `OnCreatingInternalAsync`, `OnUpdatingInternalAsync`, `OnDeletingAsync` | Before `SaveChanges`; receive the `TContext`, so extra changes are saved in the same transaction |
| Crud | `OnCreatedInternalAsync`, `OnUpdatedInternalAsync`, `OnDeletedAsync` | After saving; receive result DTO and entity |
| Crud | `GetDbSetForUpdating(TContext)`, `GetDbSetForUpdatingWithAuthorization(TContext, AuthorizationResult<TEntity>)` | Change the set used for update/delete |
| Db | `GetPendingMigrationsAsync`, `TryExecuteAsync` | Migration check / exception translation |

Full signatures: [services.md](https://github.com/wertzui/RESTworld/blob/main/doc/services.md).

## Hooks or `...InternalAsync` override?
Sequence inside the `...InternalAsync` methods (request handlers run before, response handlers after; create/update have single and collection overloads of every method and hook):
- **Create:** validators (before) → map + add → validators (after) → `OnCreatingInternalAsync` → `SaveChanges` → re-read full DTO → `OnCreatedInternalAsync`
- **Update:** load entity (404) → validators (before) → timestamp → map onto entity → validators (after) → `OnUpdatingInternalAsync` → `SaveChanges` → re-read full DTO → `OnUpdatedInternalAsync`
- **Delete:** load entity (404) → timestamp check (409) → remove → `OnDeletingAsync` → `SaveChanges` → `OnDeletedAsync`
- **Read:** read + map → `OnGotSingleInternalAsync` / `OnGotListInternalAsync` / `OnGotHistoryInternalAsync`

| Goal | Use |
|---|---|
| Change entity values or other entities in the **same transaction** | `OnCreating…` / `OnUpdating…` / `OnDeletingAsync` (get `TContext` and the tracked entity) |
| Fill or change **result DTO** properties that are not stored; call something that needs the saved data first (external service that only gets the stored id) | `OnCreated…` / `OnUpdated…` / `OnDeletedAsync` / `OnGot…` (everything storable belongs in the "ing" hooks) |
| Change the rows visible/updatable for everyone | `GetDbSetForReadingAsync` / `GetDbSetForUpdating` (per user: authorization handler) |
| Reject by content | validator ([restworld-validation](../restworld-validation/SKILL.md)) |
| Return your own status/problem, skip or replace a default step, change the query, wrap the whole operation | override `...InternalAsync`, call `base` if you only add code before/after |

**Prefer `On…ing` over `On…ed`.** The "ing" hooks get the same `TContext` that the following `SaveChangesAsync()` uses, so everything you add or change on it is stored **in one transaction with the entity** (all or nothing). Do not call `SaveChangesAsync()` in them. The "ed" hooks run **after the commit**: a second save from there is a separate transaction, and a failure leaves the first change stored and returns an error. Use an "ed" hook only when the data must already be in the database: calling an external service that only gets the stored id (`entity.Id` is generated by the database, so it is not known in `OnCreatingInternalAsync`), effects that cannot be rolled back and must not happen if saving fails (files, e-mails), or changes to the result DTO (response only, never stored).
```csharp
// Preferred: audit row saved in the same transaction (PostAudit is an example entity)
protected override Task OnUpdatingInternalAsync(AuthorizationResult<Post, PostUpdateDto> authorizationResult, BlogDatabase context, Post entity, CancellationToken cancellationToken)
{
    context.Add(new PostAudit { PostId = entity.Id, ChangedBy = GetCurrentUsersName() });
    return Task.CompletedTask;
}
// Only here: the external service needs the stored id
protected override async Task OnCreatedInternalAsync(AuthorizationResult<Post, PostCreateDto> authorizationResult, PostGetFullDto resultDto, Post entity, CancellationToken cancellationToken)
    => await _searchIndexClient.IndexAsync(entity.Id, resultDto.Headline, cancellationToken);   // example dependency
```
Hook caveats: hooks return `Task`, so they **cannot return a `ServiceResponse`**; a thrown exception becomes a `500` (or a translated `409`). "ing" hooks run before `SaveChanges` (failure writes nothing); "ed" hooks run after it (data is already committed, DTO changes are response-only). The DTO in the "ed" hooks is re-read from the database; the request DTO is `authorizationResult.Value1`. Do not run one query per item in `OnGotListInternalAsync`.

```csharp
protected override async Task OnUpdatedInternalAsync(AuthorizationResult<Post, PostUpdateDto> authorizationResult, PostGetFullDto resultDto, Post entity, CancellationToken cancellationToken)
{
    SaveImage(authorizationResult.Value1.Image);
    resultDto.Image = await GetImageAsync(resultDto.Headline, cancellationToken);
}
```
## Populate non-database properties (ExampleBlog `PostService`)
`PostGetFullDto.Image` and `Attachement` are `HalFile`s that are not mapped from the entity:

```csharp
protected override async Task<ServiceResponse<PostGetFullDto>> GetSingleInternalAsync(
    AuthorizationResult<Post, long> authorizationResult, CancellationToken cancellationToken)
{
    var result = await base.GetSingleInternalAsync(authorizationResult, cancellationToken);
    if (result.Succeeded && result.ResponseObject is not null)
    {
        result.ResponseObject.Attachement = GetAttachement(result.ResponseObject.Id);
        result.ResponseObject.Image = await GetImageAsync(result.ResponseObject.Headline, cancellationToken);
    }
    return result;
}
```
`PostService` repeats this for `CreateInternalAsync` / `UpdateInternalAsync` (single and collection) and stores uploaded files after `base` succeeded. `AuthorStatisticsService` uses the lighter hook:

```csharp
protected override async Task OnGotSingleInternalAsync(
    AuthorizationResult<Author, long> authorizationResult, AuthorStatisticsFullDto dto, CancellationToken cancellationToken)
{
    dto.TotalPosts = await _contextFactory.Parallel().Set<Post>().CountAsync(p => p.AuthorId == dto.Id, cancellationToken);
}
```
`TestService` replaces the data source: `GetDbSetForReadingAsync()` and `GetDbSetForUpdating(BlogDatabase)` return an in-memory `IQueryable<TestEntity>`, and the `...InternalAsync` write methods only log and return `ServiceResponse.FromResult(dto)`.

## Writing your own service methods
Every operation is **two methods**: the public `MyMethodAsync` contains only the `TryExecuteWithAuthorizationAsync` (or `TryExecuteAsync`) call; the real work is in the private `MyMethodInternalAsync`, which receives the request-authorization result. The wrapper runs the (Db) migration check (503), the request handlers (status ≠ OK is returned without running your code), your method, the response handlers, and turns exceptions into responses (exception translators, else 500).

**With a database (`DbServiceBase<TContext>`)**, handler: `BasicAuthorizationHandlerBase<TEntity, TRequest, TResponse>`:
```csharp
public class PublishPostAuthorizationHandler : BasicAuthorizationHandlerBase<Post, long, PostGetFullDto> { }

public class PostPublishingService : DbServiceBase<BlogDatabase>
{
    // ctor: (IDbContextFactory<BlogDatabase>, ICrudMapper<Post,...> mapper, IEnumerable<PublishPostAuthorizationHandler>, IUserAccessor, ILogger<...>)
    //       : base(contextFactory, ExceptionTranslatorFactory.CreateExceptionTranslators(mapper, contextFactory), userAccessor, logger)

    public Task<ServiceResponse<PostGetFullDto>> PublishAsync(long postId, CancellationToken cancellationToken)
        => TryExecuteWithAuthorizationAsync<Post, long, PostGetFullDto, PublishPostAuthorizationHandler>(
            postId,                                  // authorizationResult.Value1
            PublishInternalAsync,
            (result, handler, token) => handler.HandleRequestAsync(result, token),
            (response, handler, token) => handler.HandleResponseAsync(response, token),
            _authorizationHandlers, cancellationToken);

    private async Task<ServiceResponse<PostGetFullDto>> PublishInternalAsync(AuthorizationResult<Post, long> authorizationResult, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory.CreateDbContext();
        var post = await context.Set<Post>().WithAuthorizationFilter(authorizationResult)
            .FirstOrDefaultAsync(p => p.Id == authorizationResult.Value1, cancellationToken);
        if (post is null) return ServiceResponse.FromStatus<PostGetFullDto>(HttpStatusCode.NotFound);
        if (post.State == PostState.Published) return ServiceResponse.FromProblem<PostGetFullDto>(HttpStatusCode.Conflict, "The post is already published.");

        post.State = PostState.Published;
        await context.SaveChangesAsync(GetCurrentUsersName(), cancellationToken);
        var dto = await _mapper.MapEntityToFullQueryable(context.Set<Post>().Where(p => p.Id == post.Id)).SingleAsync(cancellationToken);
        return ServiceResponse.FromResult(dto);
    }
}
// Program.cs (entity must derive from ConcurrentEntityBase):
rwBuilder.AddCustomServiceAndAuthorization<Post, long, PostGetFullDto, PostPublishingService, PublishPostAuthorizationHandler>();
```
**Without a database (`ServiceBase`)**: same shape with `AuthorizationResultWithoutDb<T1>` and `BasicAuthorizationHandlerBase<TRequest, TResponse>`; no `TEntity` type argument:
```csharp
public Task<ServiceResponse<string>> GetGreetingAsync(string name, CancellationToken cancellationToken)
    => TryExecuteWithAuthorizationAsync<string, string, GreetingAuthorizationHandler>(
        name, GetGreetingInternalAsync,
        (result, handler, token) => handler.HandleRequestAsync(result, token),
        (response, handler, token) => handler.HandleResponseAsync(response, token),
        _authorizationHandlers, cancellationToken);

private Task<ServiceResponse<string>> GetGreetingInternalAsync(AuthorizationResultWithoutDb<string> authorizationResult, CancellationToken cancellationToken)
    => Task.FromResult(ServiceResponse.FromResult($"Hello {authorizationResult.Value1}"));
// Program.cs: rwBuilder.AddCustomServiceAndAuthorization<string, string, GreetingService, GreetingAuthorizationHandler>();
```
No authorization needed: `=> TryExecuteAsync(token => GetPhotoInternalAsync(email, token), cancellationToken);` (`PhotoService`).

Rules: never call `...InternalAsync` directly (it skips authorization, migration check and exception translation) · apply `.WithAuthorizationFilter(authorizationResult)` to **every** entity query or the handlers' row filters are ignored · basic handlers take **one** request value (wrap several in a record/DTO) · `SaveChangesAsync(GetCurrentUsersName(), ct)` sets `LastChangedBy` · with `RESTworld:DisableAuthorization = true`, `AddCustomServiceAndAuthorization` registers no handler, so the injected `IEnumerable<THandler>` is empty · a plain `AddScoped<THandler>()` always registers it · the controller turns the `ServiceResponse` into HTTP with `IResultFactory` ([restworld-custom-controller](../restworld-custom-controller/SKILL.md)).
## ServiceResponse / AuthorizationResult cheat sheet
- Create: `ServiceResponse.FromResult(dto)` / `FromResult(status, dto)` (2xx only), `FromStatus<T>(status)`, `FromProblem<T>(status, message)`, `FromException<T>(ex)`, `FromFailedValidation<T>(IValidationResults)` (400).
- Read: `Succeeded`, `ResponseObject`, `Status`, `ProblemDetails`, `ValidationResults`; convert with `ChangeType<TNew>()`.
- `AuthorizationResult<TEntity, T1[, T2]>`: `Value1`, `Value2`, `Status`, `Filter`; `ChangeParameter(s)`, `WithFilter(...)`, `WithStatus(...)`. A status other than `OK` stops the call and is returned to the client.

## Registration (ExampleBlog `Program.cs`)
```csharp
rwBuilder.AddCrudPipelineWithCustomServiceAndAuthorization<BlogDatabase, Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto, PostService, BlogpostAuthorizationHandler>();
rwBuilder.Services.AddCrudMapper<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto, PostMapper>();
rwBuilder.AddReadPipelineWithCustomService<BlogDatabase, Author, AuthorStatisticsListDto, AuthorStatisticsListDto, AuthorStatisticsFullDto, AuthorStatisticsService>();
services.AddScoped<MyCustomService>();                                          // used by MyCustomController
services.AddHttpClient(); services.AddScoped<IPhotoService, PhotoService>();    // used by PhotoController
```
The `...WithCustomService` overloads have no `ApiVersion` parameter.

## Exception translators
`ReadServiceBase` builds translators with `ExceptionTranslatorFactory.CreateExceptionTranslators(mapper, contextFactory)` (`RESTworld.Business.Mapping.Exceptions`). They turn a `DbUpdateConcurrencyException` into a 409 validation response and a SQL Server foreign-key violation into a failed response mapped to the DTO property. A direct `DbServiceBase` subclass must pass them itself; combine several mappers with `[.. a, .. b]` as `MyCustomService` does.

## Verification checklist
- [ ] Service derives from the right base and passes all constructor arguments to `base`.
- [ ] Only `protected virtual` members are overridden; `base` is called unless intentionally replaced.
- [ ] Changes that belong in the database are made in the "ing" hooks (same context, one transaction); "ed" hooks only for external calls that need the stored id and for result-DTO changes.
- [ ] Hooks are used for additions, `...InternalAsync` only when the default flow is replaced, rejected requests use validators.
- [ ] Custom methods: public wrapper around `TryExecuteWithAuthorizationAsync`/`TryExecuteAsync`, real code in a private `...InternalAsync`, `.WithAuthorizationFilter` on every entity query.
- [ ] Failures are returned as `ServiceResponse`, not thrown.
- [ ] Pipeline services use `Add...PipelineWithCustomService...`; other services use `AddScoped`.
- [ ] A mapper is registered for every pipeline.
- [ ] `dotnet build` succeeds and the endpoint returns the populated properties.

## Reference
- [Services](https://github.com/wertzui/RESTworld/blob/main/doc/services.md) · [Authorization](https://github.com/wertzui/RESTworld/blob/main/doc/authorization.md) · [Validation](https://github.com/wertzui/RESTworld/blob/main/doc/validation.md) · [Custom Controllers](https://github.com/wertzui/RESTworld/blob/main/doc/custom-controllers.md) · [Pipeline Overview](https://github.com/wertzui/RESTworld/blob/main/doc/pipeline-overview.md)
- Related skills: [restworld-custom-controller](../restworld-custom-controller/SKILL.md), [restworld-authorization-handler](../restworld-authorization-handler/SKILL.md), [restworld-validation](../restworld-validation/SKILL.md)
