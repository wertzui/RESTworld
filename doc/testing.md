# Testing

The `RESTworld.Testing` package helps you test the business layer of a RESTworld application — services, mappers, authorization handlers and validators — without starting a web host. A `TestBuilder` assembles a small dependency-injection container from reusable configurations and hands you the class under test (the *SUT*).

The package references EF Core InMemory and SQLite, NSubstitute and AutoFixture and is independent of the test framework. The examples use MSTest.

```bash
dotnet add package RESTworld.Testing
```

## How it works

```csharp
using var environment = new TestBuilder()
    .WithInMemoryDatabase<BlogDatabase>()
    .WithSeed(new Blog { Id = 1, Name = "First" })
    .WithoutUser()
    .WithValidation()
    .WithCrudMapper<Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto, BlogMapper>()
    .WithCrudService<BlogDatabase, Blog, BlogCreateDto, BlogQueryDto, BlogGetListDto, BlogGetFullDto, BlogUpdateDto>()
    .BuildWithServiceAsSut();

var service = environment.GetSut();
```

- Each `With…` call adds an `ITestConfiguration` (`ConfigureServices(IServiceCollection)` + `AfterConfigureServices(IServiceProvider)`). You can write your own and pass it to `With(configuration)`.
- `Build()` returns an `ITestEnvironment` (`GetService<T>()`, `GetRequiredService<T>()`, `IDisposable`).
- `Build<TSut>()` returns an `ITestEnvironment<TSut>` and registers `TSut` as a singleton if nothing else did; `GetSut()` resolves it.
- `BuildWithServiceAsSut()` is available directly after `WithCrudService`, `WithReadService` or `WithService<TService, TImplementation>()` and uses that service as the SUT.
- All services are singletons. Logging (`ILogger<T>`) is always available.
- `AfterConfigureServices` runs once the container is built; this is when seed data is written.

## Configurations

| Extension | Adds |
|---|---|
| `WithInMemoryDatabase<TContext>(string? databaseName = null)` and overloads with seed data | `IDbContextFactory<TContext>` using EF Core InMemory; a unique database per call unless you pass a name |
| `WithSqliteDatabase<TContext>(string? databaseName = null)` and overloads with seed data | `IDbContextFactory<TContext>` using in-memory SQLite; creates the schema automatically and enforces foreign keys |
| `.WithSeed(params object?[])` / `.WithSeed(IEnumerable<object?>)` (on either database builder) | entities saved after the container is built |
| `WithUser(name, authenticationType)`, `WithUser(claims, authenticationType)`, `WithUser(ClaimsIdentity)`, `WithUser(ClaimsPrincipal)` | an `IUserAccessor` returning that user (default name `"TestName"`) |
| `WithoutUser()` | an `IUserAccessor` with an unauthenticated principal |
| `WithCrudMapper<…, TMapper>(TMapper? instance = null)`, `WithReadMapper<…, TMapper>(…)` | your mapper as `ICrudMapper`/`IReadMapper` |
| `WithCrudAutoMapper<…>()`, `WithReadAutoMapper<…>()`, `WithAutomapper(cfg => …)` | Legacy: AutoMapper-based mapping (only for code that has not been migrated to Mapperly yet) |
| `WithValidation()` | `IValidationService<,,>` (open generic) |
| `WithValidation(validator)` (create, update and combined overloads) | the validation service plus your validator instance |
| `WithCrudService<TContext, TEntity, TCreate, TQuery, TList, TFull, TUpdate>()` and shorter overloads | `ICrudServiceBase<…>` implemented by `CrudServiceBase<…>` |
| `WithReadService<TContext, TEntity, TQuery, TList, TFull>()` and shorter overloads | `IReadServiceBase<…>` implemented by `ReadServiceBase<…>` |
| `WithService<TService, TImplementation>()`, `WithService<TImpl>()`, `WithService(instance)`, `WithService<T>(Func<IServiceProvider, T>)` | any service |
| `WithSubstitute<T>(Action<T>? configure = null)`, `.Configure(Action<T>)` | an NSubstitute substitute |
| `WithOptions<T>(T options)` / `WithOptions<T>(Action<T>)` | `IOptions<T>` |
| `WithAction(Action<IServiceCollection>)`, `WithAction(Action<IServiceProvider>)`, `WithAction(configure, after)` | arbitrary setup; type the lambda parameter to pick the overload |

## Using SQLite

Use SQLite when tests need relational queries, foreign-key checks or transactions without a database server:

```csharp
using var environment = new TestBuilder()
    .WithSqliteDatabase<BlogDatabase>()
    .WithSeed(new Blog { Id = 1, Name = "First" })
    .Build();

var factory = environment.GetRequiredService<IDbContextFactory<BlogDatabase>>();
using var context = factory.CreateDbContext();
var blog = await context.Set<Blog>().SingleAsync();
```

`SqliteDatabaseTestConfiguration<TContext>` and `SqliteDatabaseTestBuilderExtensions` mirror the InMemory configuration's constructors, database-name/seed overloads and `WithSeed` methods. Null seed entries are ignored. The schema is created with `EnsureCreated`, not migrations, even without seed data.

By default, each configuration uses a unique in-memory database. An explicit name shares data between environments while at least one remains alive; empty/whitespace names and the reserved `:memory:` name are rejected. A provider-owned connection keeps the database alive, and each context uses its own connection. Dispose contexts before the environment; the database disappears after the last connection closes. Pooling is disabled so disposed connections cannot retain test data. Setup failures propagate and dispose the environment's services.

## Testing a service

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

Services return `ServiceResponse<T>` (`Succeeded`, `Status`, `ResponseObject`, `ProblemDetails`, `ValidationResults`), so you assert on the result instead of catching exceptions.

To run the service together with an authorization handler, register it for the handler interface before the service:

```csharp
.WithService<ICrudAuthorizationHandler<Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>, BlogpostAuthorizationHandler>()
.WithCrudService<BlogDatabase, Post, PostCreateDto, PostQueryDto, PostListDto, PostGetFullDto, PostUpdateDto>()
```

Without any handler registered the service logs a warning once and allows everything.

To assert on the stored data, use `IDbContextFactory<T>` from the environment:

```csharp
var factory = environment.GetRequiredService<IDbContextFactory<BlogDatabase>>();
var count = await factory.Parallel().Set<Blog>().CountAsync();
```

## Testing a mapper

Mapperly mappers are plain classes:

```csharp
var mapper = new BlogMapper();
var entity = mapper.MapCreateToEntity(new BlogCreateDto { Name = "x" });
var dto = mapper.MapEntityToFullExpression().Compile()(new Blog { Id = 3, Name = "n" });
```

Only for legacy AutoMapper configurations use `WithAutomapper(cfg => cfg.CreateMap<TestModel, TestDto>())` and resolve `IMapper` from the environment.

## Testing an authorization handler

Call the handler directly and pass in an `AuthorizationResult`:

```csharp
using var environment = new TestBuilder().Build<BlogpostAuthorizationHandler>();
var handler = environment.GetSut();

var result = await handler.HandleGetSingleRequestAsync(
    AuthorizationResult.Ok<Post, long>(404), CancellationToken.None);

Assert.AreEqual(HttpStatusCode.NotFound, result.Status);
```

- Filters added with `WithFilter(...)` are exposed as `result.Filter`; apply them to an `IQueryable<TEntity>` in the test.
- Response handlers take a `ServiceResponse<T>`: `handler.HandleGetSingleResponseAsync(ServiceResponse.FromResult(new PostGetFullDto { Id = 42 }), …)`.
- Handlers that depend on the current user need `.WithUser("alice")` or `.WithoutUser()` (for instance `UserIsAuthorizedCrudAuthorizationHandler<…>` returns `HttpStatusCode.Unauthorized` for an unauthenticated user).

See [Authorization](authorization.md) for writing handlers.

## Testing a validator

```csharp
using var environment = new TestBuilder()
    .WithInMemoryDatabase<BlogDatabase>()
    .Build<PostValidatior>();

var results = await environment.GetSut().ValidateBeforeCreateAsync(
    new PostCreateDto { AuthorId = 1, Headline = "a", Text = "A" }, CancellationToken.None);

Assert.IsFalse(results.ValidationSucceeded);
```

To test validation through the whole service use `WithValidation(validator)` together with `WithCrudService`.

## Limits

- EF Core InMemory is not SQL Server: there are no `Timestamp` row versions and no referential integrity checks. SQLite enforces foreign keys and supports transactions, but differs from SQL Server in SQL translation, supported types and concurrency; it does not generate SQL Server `Timestamp` row versions. Test provider-specific behavior against your production database.
- Controllers, HAL/HAL-Forms output and routing are not part of this package; test those with an ASP.NET Core integration test if needed.

Related: [Pipeline Overview](pipeline-overview.md), [Mapping and Versioning](mapping-and-versioning.md), [Authorization](authorization.md).
