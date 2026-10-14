# Entities and Database

RESTworld stores data with Entity Framework Core on SQL Server. Your entities derive from small base classes that provide an identifier, optimistic concurrency and audit columns, and your `DbContext` derives from `DbContextBase` so RESTworld can apply concurrency detection and fill the audit columns automatically.

> Entities and the `DbContext` live in the `MyApi.Data` project of the [recommended solution layout](getting-started.md#recommended-solution-structure). Entities are exposed to clients through DTOs — see [DTO Types and Roles](dto-types.md) and [Mapping and Versioning](mapping-and-versioning.md).

## Entity base classes

All base classes are in the namespace `RESTworld.EntityFrameworkCore.Models`.

| Base class | Adds | Use when |
|---|---|---|
| `EntityBase` | `long Id` | Read-only endpoints (`AddReadPipeline...` only requires `EntityBase`) |
| `ConcurrentEntityBase` | `Id`, `byte[]? Timestamp` | CRUD endpoints (`AddCrudPipeline...` requires `ConcurrentEntityBase`) |
| `ChangeTrackingEntityBase` | `Id`, `Timestamp`, `DateTimeOffset? CreatedAt`, `string? CreatedBy`, `DateTimeOffset? LastChangedAt`, `string? LastChangedBy` | CRUD endpoints with audit information (recommended default) |

> `ChangeTrackingEntityBase` extends `ConcurrentEntityBase`, which extends `EntityBase`. The matching DTO base classes (`DtoBase`, `ConcurrentDtoBase`, `ChangeTrackingDtoBase`) are described in [DTO Types and Roles](dto-types.md#base-classes).

```csharp
using RESTworld.EntityFrameworkCore.Models;

namespace ExampleBlog.Data.Models;

public class Blog : ChangeTrackingEntityBase
{
    public required string Name { get; set; }
    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
}

public class Post : ChangeTrackingEntityBase
{
    public virtual Author Author { get; set; } = default!;
    public long AuthorId { get; set; }
    public virtual Blog Blog { get; set; } = default!;
    public long BlogId { get; set; }
    public required string Headline { get; set; }
    public PostState State { get; set; }
    public required string Text { get; set; }
}
```

Relations are modelled in the usual EF Core way: a scalar foreign key (`long AuthorId`), a navigation property (`Author`) and a collection on the principal side. Enums used by entities and DTOs belong into the `.Common` project so both can reference them. If the entities of an endpoint reference each other, the endpoint needs a [custom service](choosing_a_pipeline.md) to load and map the related data.

## Concurrency detection

`Timestamp` is mapped to a SQL Server `rowversion` column (`[Timestamp]`). The DTOs returned to clients contain the current value, and clients send it back on update and delete. `DbContextBase` replaces the original value of the tracked `Timestamp` with the one received from the client before saving (`TimestampConcurrencyDetection`), so EF Core detects a concurrent change and RESTworld answers with **HTTP 409 Conflict**. Foreign key violations are also translated to 409 responses with a descriptive message.

You do not have to implement anything for this. Just never map the `Timestamp` from a DTO to the entity yourself — the mapper ignores it (see [Mapping and Versioning](mapping-and-versioning.md)).

## Audit columns

For entities derived from `ChangeTrackingEntityBase`, `DbContextBase.SaveChangesAsync(string? currentUser, ...)` writes:

- On insert: `CreatedAt`, `CreatedBy`, `LastChangedAt`, `LastChangedBy`
- On update: `LastChangedAt`, `LastChangedBy` (`CreatedAt`/`CreatedBy` are protected against modification)

The default services call this overload with the current user from `IUserAccessor`. If you write your own code that saves entities, use `SaveChangesAsync(currentUser, cancellationToken)` instead of the parameterless EF Core method, otherwise `LastChangedBy` stays empty.

## The DbContext

Derive from `DbContextBase` (`RESTworld.EntityFrameworkCore`) and add a `DbSet<T>` for every entity:

```csharp
using Microsoft.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore;

namespace ExampleBlog.Data;

public class BlogDatabase : DbContextBase
{
    public BlogDatabase(DbContextOptions<BlogDatabase> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
}
```

`DbContextBase` also offers a virtual `Migrate()` method that applies migrations without a command timeout, so long running migrations do not fail.

### Registration

```csharp
var rwBuilder = builder.AddRestWorld();

rwBuilder.AddDbContextFactoryWithDefaults<BlogDatabase>();

// Optional: apply pending migrations in the background while the application starts.
rwBuilder.MigrateDatabaseDuringStartup<BlogDatabase>();
```

`AddDbContextFactoryWithDefaults<TContext>()` registers a pooled `IDbContextFactory<TContext>`:

- The connection string is read from `ConnectionStrings:<name of the context class>` — for `BlogDatabase` this is `ConnectionStrings:BlogDatabase`.
- Without custom options it uses SQL Server with `EnableRetryOnFailure()`, `EnableDetailedErrors()` and `EnableSensitiveDataLogging()`.
- You can pass `Action<DbContextOptionsBuilder> optionsAction` and/or `Action<SqlServerDbContextOptionsBuilder> sqlServerOptionsAction`. If you provide `optionsAction`, **none** of the defaults are applied, including `UseSqlServer`, so you have to configure the provider yourself.
- It adds the [database health checks](#health-checks) described below.
- It enables temporal queries to be joined with non-temporal entities.

`MigrateDatabaseDuringStartup<TContext>()` adds a hosted service that applies pending migrations in the background. The application can already answer requests that do not need the database while the migration runs; the `startup` health check reports unhealthy until all migrations are applied. A failing migration is logged as critical. When you deploy migrations differently (for example with a migration bundle), leave this call out.

```json
{
  "ConnectionStrings": {
    "BlogDatabase": "Server=(localdb)\\mssqllocaldb;Database=BlogDatabase;integrated security=true;"
  }
}
```

When you run under Aspire, `builder.AddConnectionString("BlogDatabase")` plus `.WithReference(database)` injects the same setting as `ConnectionStrings__BlogDatabase`. See [Health and Operations](health-and-operations.md).

## Migrations

The EF Core tools must be able to create the context without starting the web application. Add a design-time factory to the `.Data` project:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExampleBlog.Data;

public class BlogContextFactory : IDesignTimeDbContextFactory<BlogDatabase>
{
    public BlogDatabase CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogDatabase>();
        optionsBuilder.UseSqlServer();

        return new BlogDatabase(optionsBuilder.Options);
    }
}
```

RESTworld also ships a generic implementation, so you can write `public class BlogContextFactory : SqlServerDesignTimeDbContextFactory<BlogDatabase> { }` (`RESTworld.EntityFrameworkCore`). The context needs a public constructor with a single `DbContextOptions<TContext>` parameter for this to work.

The `.Data` project needs a reference to `Microsoft.EntityFrameworkCore.Design`. Install the tool once and create migrations from the solution folder:

```powershell
dotnet new tool-manifest
dotnet tool install dotnet-ef

dotnet ef migrations add AddPosts --project src\MyApi.Data --startup-project src\MyApi.Data
dotnet ef database update --project src\MyApi.Data --startup-project src\MyApi.Data
```

Always review the generated migration. A renamed property is generated as dropping and adding a column, which loses data — edit it to a `RenameColumn` call. Migrations are also the place where database changes for a new [API version](mapping-and-versioning.md#versioning-strategy) happen (the ExampleBlog migration `AuthorV2` is an example).

### Seed data

Use `HasData` in `OnModelCreating` to ship initial data. `HasData` does not go through `SaveChanges`, so set the `Id` and the audit columns yourself:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>(entity =>
    {
        entity.HasData(
            new Blog { Id = 1, Name = "My first Blog", CreatedAt = ..., CreatedBy = "me", LastChangedAt = ..., LastChangedBy = "me" });
    });
}
```

ExampleBlog wraps this in a small internal extension method (`AddDefaults()` in `DefaultDataExtensions`) that fills the audit columns for any `ChangeTrackingEntityBase`.

## Temporal tables and history

SQL Server temporal tables keep the previous versions of every row. To use them:

1. Mark the table as temporal in `OnModelCreating` and create a migration:

   ```csharp
   modelBuilder.Entity<Post>(entity =>
   {
       entity.ToTable(b => b.IsTemporal());
   });
   ```

2. Put the `[HasHistory]` attribute on the `TGetFullDto` of the endpoint:

   ```csharp
   [HasHistory]
   public class PostGetFullDto : ChangeTrackingDtoBase
   {
       // ...
   }
   ```

   The read controller then offers a `history` endpoint (`GET /Post/history`) that accepts the OData options of the `TQueryDto` plus `$at` (a point in time), or `$from` together with `$to` (exclusive) or `$toInclusive`. Without `[HasHistory]` the endpoint answers 404 and is hidden from Swagger.

3. In your own queries use the extension methods in `RESTworld.EntityFrameworkCore` (namespace `Microsoft.EntityFrameworkCore`): `TemporalAsOf`, `TemporalFromTo`, `TemporalBetween`, `TemporalContainedIn` and `TemporalAll`. Times are stored in UTC and temporal queries are not tracked.

## Health checks

`AddDbContextFactoryWithDefaults<TContext>()` registers two health checks per context, unless they already exist:

| Name | Tag | Healthy when |
|---|---|---|
| `<Context>Migration` | `startup` | the database has no pending migrations |
| `<Context>Connection` | `ready` | a connection to the database can be opened |

They are served through the Kubernetes style endpoints `/health/startup` and `/health/ready` that `UseRestWorld()` maps (and `/health/live`, which has no checks by default). Use `WithKubernetesHealthProbes()` in the Aspire AppHost to wire them as probes. More details in [Health and Operations](health-and-operations.md).

## Next steps

- Create the DTOs for your entity in [DTO Types and Roles](dto-types.md).
- Map entities and DTOs as described in [Mapping and Versioning](mapping-and-versioning.md).
- Choose and register a pipeline with [Choosing a Pipeline](choosing_a_pipeline.md) and [Getting Started](getting-started.md).
