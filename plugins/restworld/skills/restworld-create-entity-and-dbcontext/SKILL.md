---
name: restworld-create-entity-and-dbcontext
description: 'Create or extend RESTworld Entity Framework Core entities and the DbContext. Use when asked to create an entity, add a database table, add a model class, add a DbSet, create or change a DbContext, derive from DbContextBase, choose between EntityBase / ConcurrentEntityBase / ChangeTrackingEntityBase, add a foreign key or navigation property, make a table temporal (SQL Server history), add seed data with HasData, add an EF Core migration, run dotnet ef, add a design-time DbContext factory, register the database with AddDbContextFactoryWithDefaults, or apply migrations on startup with MigrateDatabaseDuringStartup. Also covers the connection string name, rowversion Timestamp concurrency, CreatedAt/LastChangedBy auditing and where the files belong in the .Data project.'
argument-hint: 'Describe the entity (name, properties, relations to other entities). Optionally say whether it needs history (temporal table), seed data, or which .Data project and DbContext to use.'
---

# RESTworld: Create an Entity and DbContext

## When to Use
- You are adding a new table/entity to a RESTworld API
- You are creating the first `DbContext` of a new solution
- You need auditing columns, optimistic concurrency, a temporal (history) table, seed data or a migration

Next steps after the entity exists: [restworld-create-dtos](../restworld-create-dtos/SKILL.md), [restworld-create-mapper](../restworld-create-mapper/SKILL.md), [restworld-choose-pipeline](../restworld-choose-pipeline/SKILL.md). The whole flow is in [restworld-add-entity-endpoint](../restworld-add-entity-endpoint/SKILL.md).

## Rules
1. Entities and the DbContext live in the **`MyApi.Data`** project (references `RESTworld.EntityFrameworkCore`, `MyApi.Common`, and `Microsoft.EntityFrameworkCore.Design`). Entities go in `Models/`, migrations in `Migrations/`.
2. Pick the base class from `RESTworld.EntityFrameworkCore.Models`:

   | Base class | Adds | Use for |
   |---|---|---|
   | `EntityBase` | `long Id` | Read-only pipelines (`AddReadPipeline...` accepts `EntityBase`) |
   | `ConcurrentEntityBase` | `Id` + `byte[]? Timestamp` (`[Timestamp]`, SQL `rowversion`) | CRUD pipelines require at least this |
   | `ChangeTrackingEntityBase` | `Timestamp` + `CreatedAt`, `CreatedBy`, `LastChangedAt`, `LastChangedBy` | **Default choice** for CRUD entities |

   They inherit in that order (`ChangeTrackingEntityBase : ConcurrentEntityBase : EntityBase`). CRUD pipelines have the constraint `TEntity : ConcurrentEntityBase`.
3. The `DbContext` must derive from **`DbContextBase`** (`RESTworld.EntityFrameworkCore`). It provides timestamp concurrency detection (a stale `Timestamp` results in HTTP 409), sets the auditing columns when saving with a user, and removes the command timeout in `Migrate()`.
4. Use a single public constructor taking `DbContextOptions<TContext>`. The context is registered as a **pooled** factory, so keep it free of per-request state.
5. Foreign keys: scalar `long XxxId` **plus** `virtual Xxx Xxx` navigation, and a `virtual ICollection<T>` (initialised with `new HashSet<T>()`) on the principal side. An endpoint whose entities have FKs to each other needs a custom service later (see restworld-choose-pipeline, Rule A).
6. Never take `Id`, `Timestamp` or the auditing columns from a DTO; the mapper ignores them (see [restworld-create-mapper](../restworld-create-mapper/SKILL.md)).
7. The connection string is read from `ConnectionStrings:<DbContext class name>`.

## Steps

### 1. Entity (`MyApi.Data/Models/Post.cs`, from ExampleBlog)
```csharp
using RESTworld.EntityFrameworkCore.Models;

namespace ExampleBlog.Data.Models;

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
Enums used by entities and DTOs belong in the `.Common` project (`ExampleBlog.Common.Enums`).

### 2. DbContext (`BlogDatabase.cs`)
```csharp
public class BlogDatabase : DbContextBase
{
    public BlogDatabase(DbContextOptions<BlogDatabase> options) : base(options) { }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable(b => b.IsTemporal()); // optional: SQL Server temporal (history) table
        });
    }
}
```
Add a `DbSet<T>` for every new entity.

### 3. Optional: history (temporal table)
- `entity.ToTable(b => b.IsTemporal());` in `OnModelCreating`, then add a migration (ExampleBlog: `PostHistory`).
- Put `[HasHistory]` (namespace `System.ComponentModel.DataAnnotations`, from `RESTworld.Common`) on the **`TGetFullDto`** to expose the `/history` endpoint (`$at`, `$from`, `$to`, `$toInclusive`). `PostGetFullDto` does this.
- In custom queries use `TemporalAsOf`, `TemporalFromTo`, `TemporalBetween`, `TemporalContainedIn`, `TemporalAll` (extension methods on `IQueryable<T>` in namespace `Microsoft.EntityFrameworkCore`).
- `AddDbContextFactoryWithDefaults` already replaces `INavigationExpansionExtensibilityHelper` so temporal queries can join non-temporal entities.

### 4. Optional: seed data
`HasData` bypasses `SaveChanges`, so set `Id` and the auditing columns yourself. ExampleBlog uses an internal helper (`DefaultDataExtensions.AddDefaults<T>()`, sets `CreatedAt/CreatedBy/LastChangedAt/LastChangedBy` on any `ChangeTrackingEntityBase`):
```csharp
modelBuilder.Entity<Blog>(entity => entity.HasData(
    new Blog { Id = 1, Name = "My first Blog" }.AddDefaults()));
```

### 5. Design-time factory (once per context, in the `.Data` project)
EF tooling needs to create the context without running the web app. ExampleBlog:
```csharp
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
Shorter alternative: `public class BlogContextFactory : SqlServerDesignTimeDbContextFactory<BlogDatabase> { }` (RESTworld base class in `RESTworld.EntityFrameworkCore`; creates the context via `Activator.CreateInstance(typeof(TDbContext), options)`).

### 6. Register in `Program.cs`
```csharp
rwBuilder.AddDbContextFactoryWithDefaults<BlogDatabase>();   // pooled IDbContextFactory + health checks
rwBuilder.MigrateDatabaseDuringStartup<BlogDatabase>();      // optional, applies pending migrations in the background
```
- Both are `IHostApplicationBuilder` extensions (namespace `Microsoft.Extensions.DependencyInjection`). Optional parameters of the first: `Action<DbContextOptionsBuilder>? optionsAction`, `Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction`.
- Defaults: SQL Server with `EnableRetryOnFailure()`, `EnableDetailedErrors()`, `EnableSensitiveDataLogging()`. If you pass `optionsAction`, **none of these defaults (including `UseSqlServer`) are applied**; call `UseSqlServer(...)` yourself.
- Connection string in `appsettings.json`: `"ConnectionStrings": { "BlogDatabase": "Server=(localdb)\\mssqllocaldb;Database=BlogDatabase;integrated security=true;" }`.
- Health checks `<Context>Migration` (tag `startup`) and `<Context>Connection` (tag `ready`) are added automatically.

### 7. Migration
One-time tool setup (ExampleBlog keeps it in `ExampleBlog.Client/dotnet-tools.json`): `dotnet new tool-manifest` then `dotnet tool install dotnet-ef`.
```powershell
dotnet ef migrations add AddPost --project src\MyApi.Data --startup-project src\MyApi.Data
dotnet ef database update --project src\MyApi.Data --startup-project src\MyApi.Data   # optional, startup migration does this
```
Review the generated migration (a property rename is generated as drop + add; edit it to `RenameColumn` to keep data). Migrations caused by DTO-shape changes: see [restworld-add-api-version](../restworld-add-api-version/SKILL.md).

## Verification Checklist
- [ ] Entity derives from `ChangeTrackingEntityBase` (CRUD) or at least `EntityBase` (read-only)
- [ ] `DbSet<T>` added to the context deriving from `DbContextBase`
- [ ] FK has both `long XId` and navigation property
- [ ] Design-time factory exists in the `.Data` project
- [ ] Migration generated, reviewed and committed together with the model snapshot
- [ ] `AddDbContextFactoryWithDefaults<T>()` and (optionally) `MigrateDatabaseDuringStartup<T>()` in `Program.cs`; connection string key equals the context class name
- [ ] `dotnet build` succeeds

## Reference
- [Entities and Database](https://github.com/wertzui/RESTworld/blob/main/doc/entities-and-database.md)
- [Getting Started](https://github.com/wertzui/RESTworld/blob/main/doc/getting-started.md)
- [Health and Operations](https://github.com/wertzui/RESTworld/blob/main/doc/health-and-operations.md)
- [DTO Types and Roles](https://github.com/wertzui/RESTworld/blob/main/doc/dto-types.md)
