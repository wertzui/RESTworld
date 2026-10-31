---
name: restworld-testing
description: 'Write unit and integration-style tests for a RESTworld application with the RESTworld.Testing package. Use when asked to test a RESTworld service, mapper, authorization handler, validator or custom service; to set up TestBuilder, ITestEnvironment, ITestConfiguration; to use WithInMemoryDatabase, WithSqliteDatabase, WithSeed, WithUser, WithoutUser, WithCrudService, WithReadService, WithCrudMapper, WithReadMapper, WithCrudAutoMapper, WithAutomapper, WithValidation, WithService, WithSubstitute, WithOptions, WithAction or BuildWithServiceAsSut; or to create a test project for a RESTworld API. Also handles: seeding an in-memory database, testing SQLite relational behavior, faking the current user, substituting dependencies with NSubstitute, and testing authorization/validation outcomes through the real service.'
argument-hint: 'Name the class under test (service, mapper, authorization handler, validator) and the test framework in use (default MSTest). Optionally paste the class.'
---

# RESTworld: Testing with RESTworld.Testing

`RESTworld.Testing` (NuGet package, namespace `RESTworld.Testing`) builds a small DI container per test, wires the usual RESTworld dependencies for you and returns the class under test (the *SUT*). It references EF Core InMemory and SQLite, NSubstitute, AutoFixture and console logging. It is test-framework agnostic; the examples use MSTest.

## When to Use
- Testing a service derived from `CrudServiceBase` / `ReadServiceBase` / `DbServiceBase`, a mapper, an authorization handler or a validator
- Setting up a test project for a RESTworld solution
- Checking that an authorization handler or validator really changes the service result

## Key Rules

1. **Compose with `new TestBuilder()`**, chain `With…` extension methods, finish with `Build()`, `Build<TSut>()` or `BuildWithServiceAsSut()`. Dispose the environment (`using var`), `ITestEnvironment` is `IDisposable`.
2. **`Build<TSut>()`** registers `TSut` as singleton if nothing registered it, then `GetSut()` resolves it. All constructor dependencies of the SUT must have been added by `With…` calls. `ILogger<T>` is always available (logging is added automatically).
3. **`BuildWithServiceAsSut()`** exists only directly after `WithCrudService(…)`, `WithReadService(…)` or `WithService<TService, TImplementation>()` (it returns `ITestEnvironment<TService>`). Do not insert other `With…` calls between.
4. **Order of registration does not matter for resolution**, but `WithSeed` must follow `WithInMemoryDatabase<TContext>()` or `WithSqliteDatabase<TContext>()` (it extends those builders). Seeding runs after the container is built.
5. **Each database configuration gets a unique database name** (GUID) unless you pass one, so tests are isolated. SQLite automatically creates the schema and keeps its in-memory database alive until all contexts and environments using that name are disposed. Empty/whitespace names and `:memory:` are rejected; use null for an automatic name.
6. **Everything is registered as singleton.**
7. **Use `Parallel()`** (from `EntityFrameworkCore.Parallel`, `using Microsoft.EntityFrameworkCore;`) on `IDbContextFactory<T>` in assertions: `await factory.Parallel().Set<Post>().CountAsync()`.
8. The InMemory provider is not SQL Server: it does not generate `Timestamp` row versions or enforce relational constraints. SQLite enforces foreign keys and supports transactions, but does not generate SQL Server row versions either. Test production-provider-specific behavior against that database separately.

## Builder cheat sheet

| Extension | Registers |
|---|---|
| `WithInMemoryDatabase<TContext>(string? databaseName = null)` (+ overloads taking seed data) | `IDbContextFactory<TContext>` on EF InMemory |
| `WithSqliteDatabase<TContext>(string? databaseName = null)` (+ overloads taking seed data) | `IDbContextFactory<TContext>` on in-memory SQLite with automatic schema creation |
| `.WithSeed(params object?[])` / `.WithSeed(IEnumerable<object?>)` | Entities added after build on either database builder |
| `WithUser(string? name = null, string? authenticationType = null)`, `WithUser(IEnumerable<Claim>?, string?)`, `WithUser(ClaimsIdentity)`, `WithUser(ClaimsPrincipal)` | `IUserAccessor` with that principal (default name `"TestName"`) |
| `WithoutUser()` | `IUserAccessor` with an unauthenticated principal |
| `WithCrudMapper<TEntity, TCreate, TQuery, TList, TFull, TUpdate, TMapper>(TMapper? instance = null)` | `ICrudMapper`, `IReadMapper` and `TMapper` |
| `WithReadMapper<TEntity, TQuery, TList, TFull, TMapper>(…)` | `IReadMapper` and `TMapper` |
| `WithCrudAutoMapper<…6 types…>()` / `WithReadAutoMapper<…4 types…>()` | legacy AutoMapper-based mappers (need `WithAutomapper`; for code not yet migrated to Mapperly) |
| `WithAutomapper(Action<IMapperConfigurationExpression>)` | `IMapper` (legacy AutoMapper only) |
| `WithValidation()` | `IValidationService<,,>` (open generic) |
| `WithValidation<TCreate, TUpdate, TEntity>(IValidator<…> validator)` (also create-only / update-only overloads) | validation service + your validator instance |
| `WithCrudService<TContext, TEntity, TCreate, TQuery, TList, TFull, TUpdate>()` (+ shorter overloads) | `ICrudServiceBase<…>` → `CrudServiceBase<…>` |
| `WithReadService<TContext, TEntity, TQuery, TList, TFull>()` (+ shorter overloads) | `IReadServiceBase<…>` → `ReadServiceBase<…>` |
| `WithService<TService, TImpl>()`, `WithService<TImpl>()`, `WithService<TService, TImpl>(instance)`, `WithService<T>(Func<IServiceProvider, T>)` | any service |
| `WithSubstitute<T>(Action<T>? configure = null)` then `.Configure(Action<T>)` | an NSubstitute substitute |
| `WithOptions<T>(T options)` / `WithOptions<T>(Action<T>)` | `IOptions<T>` |
| `WithAction(Action<IServiceCollection>)`, `WithAction(Action<IServiceProvider>)`, `WithAction(configure, after)` | anything else (see below on overload resolution) |

When using lambdas with `WithAction`, give the parameter a type (`(IServiceCollection s) => …`) to disambiguate the overloads.

## Examples (consumer test project, MSTest)

### Service test (CRUD service on an in-memory database)
```csharp
[TestClass]
public class BlogServiceTests
{
    [TestMethod]
    public async Task Create_then_get_returns_the_blog()
    {
        using var environment = new TestBuilder()
            .WithInMemoryDatabase<BlogDatabase>()
            .WithSeed(new Blog { Id = 1, Name = "First" })
            .WithoutUser()
            .WithValidation()
            .WithCrudMapper<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto, BlogMapper>()
            .WithCrudService<BlogDatabase, Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>()
            .BuildWithServiceAsSut();
        var service = environment.GetSut();

        var created = await service.CreateAsync(new BlogCreateDto { Name = "Second" }, CancellationToken.None);
        var missing = await service.GetSingleAsync(999, CancellationToken.None);

        Assert.IsTrue(created.Succeeded, created.ProblemDetails);
        Assert.AreEqual("Second", created.ResponseObject!.Name);
        Assert.AreEqual(HttpStatusCode.NotFound, missing.Status);
    }
}
```
Without an authorization handler registered the service logs a warning once and allows everything. To test with a handler, add `.WithService<ICrudAuthorizationHandler<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>, BlogpostAuthorizationHandler>()` (before `WithCrudService`) — then `service.GetSingleAsync(402, …)` returns `HttpStatusCode.PaymentRequired` through `BlogpostAuthorizationHandler`.

### Mapper test (Mapperly mappers need no container)
```csharp
[TestMethod]
public void MapCreateToEntity_copies_the_name()
{
    var mapper = new BlogMapper();

    var entity = mapper.MapCreateToEntity(new BlogCreateDto { Name = "x" });
    var dto = mapper.MapEntityToFullExpression().Compile()(new Blog { Id = 3, Name = "n" });

    Assert.AreEqual("x", entity.Name);
    Assert.AreEqual("n", dto.Name);
}
```
For legacy AutoMapper-based mappers only, use `.WithAutomapper(cfg => cfg.CreateMap<TestModel, TestDto>())` and resolve `IMapper` via `environment.GetRequiredService<IMapper>()`.

### Authorization handler test (call the handler directly)
```csharp
[TestMethod]
public async Task GetSingle_with_an_http_status_code_id_short_circuits()
{
    using var environment = new TestBuilder().Build<BlogpostAuthorizationHandler>(); // needs only ILogger<>
    var handler = environment.GetSut();

    var result = await handler.HandleGetSingleRequestAsync(
        AuthorizationResult.Ok<Post, long>(404), CancellationToken.None);

    Assert.AreEqual(HttpStatusCode.NotFound, result.Status);
}

[TestMethod]
public async Task Response_for_post_42_is_a_problem()
{
    using var environment = new TestBuilder().Build<BlogpostAuthorizationHandler>();

    var response = await environment.GetSut().HandleGetSingleResponseAsync(
        ServiceResponse.FromResult(new PostGetFullDto { Id = 42 }), CancellationToken.None);

    Assert.AreEqual((HttpStatusCode)42, response.Status);
}
```
Filters set with `WithFilter(...)` are applied via `result.Filter(queryable)`. For handlers that read the user (e.g. `UserIsAuthorizedCrudAuthorizationHandler<…>` or your own with `IUserAccessor`) add `.WithUser("alice")` or `.WithoutUser()` — the latter makes `HandleCreateRequestAsync(...)` return `HttpStatusCode.Unauthorized`.

### Validator test
```csharp
[TestMethod]
public async Task Headline_and_text_must_differ()
{
    using var environment = new TestBuilder()
        .WithInMemoryDatabase<BlogDatabase>()
        .Build<PostValidatior>();

    var results = await environment.GetSut().ValidateBeforeCreateAsync(
        new PostCreateDto { AuthorId = 1, Headline = "a", Text = "A" }, CancellationToken.None);

    Assert.IsFalse(results.ValidationSucceeded);
}
```

### Substituting a dependency
```csharp
using var environment = new TestBuilder()
    .WithSubstitute<IPhotoService>(s => s.GetPhotoAsync("a@b.c", Arg.Any<CancellationToken>())
        .Returns(ServiceResponse.FromResult<Stream>(new MemoryStream())))
    .Build<MyThingThatUsesPhotos>();
```

## Steps for a new test project
1. Create a test project (`net10.0`), add package references `RESTworld.Testing` and your test framework; reference your `.Business`, `.Data` and `.Common` projects.
2. Add one test class per service/handler/validator/mapper; build the environment inside each test (it is cheap).
3. Run `dotnet test`.

## Verification Checklist
- [ ] Environment is created per test and disposed
- [ ] `BuildWithServiceAsSut()` directly follows the `WithCrudService` / `WithReadService` / `WithService` call
- [ ] All SUT constructor dependencies are registered (mapper, `IUserAccessor` via `WithUser`/`WithoutUser`, validation, authorization handler where relevant)
- [ ] Database state is asserted with `factory.Parallel()`
- [ ] No assertions rely on SQL Server-only behavior (rowversion); use SQLite or the production database for foreign-key enforcement

## Related
- [Testing guide](https://github.com/wertzui/RESTworld/blob/main/doc/testing.md)
- Skills `restworld-custom-service`, `restworld-authorization-handler`, `restworld-validation`, `restworld-create-mapper`
