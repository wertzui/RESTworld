using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTworld.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTworld.Tests.Testing;

[TestClass]
public class SqliteDatabaseTestConfigurationTests
{
    [TestMethod]
    public void WithSqliteDatabase_should_register_factory_and_context_and_create_an_empty_schema()
    {
        using var environment = new TestBuilder()
            .WithSqliteDatabase<SqliteTestDatabase>()
            .Build();

        var factory = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>();
        using var context = factory.CreateDbContext();
        Assert.AreEqual("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
        Assert.AreEqual(0, context.Parents.Count());
        Assert.AreEqual(0, environment.GetRequiredService<SqliteTestDatabase>().Children.Count());
    }

    [TestMethod]
    public async Task WithSeed_should_combine_params_and_enumerable_data_and_ignore_nulls()
    {
        using var environment = new TestBuilder()
            .WithSqliteDatabase<SqliteTestDatabase>(new Parent { Id = 1, Name = "First" }, null)
            .WithSeed(new Parent { Id = 2, Name = "Second" }, null)
            .WithSeed((IEnumerable<object?>)new object?[] { new Parent { Id = 3, Name = "Third" }, null })
            .WithSeed((object?[]?)null)
            .WithSeed((IEnumerable<object?>?)null)
            .Build();

        var factory = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>();
        var count = await factory.Parallel().Set<Parent>().CountAsync();
        Assert.AreEqual(3, count);
        using var context = factory.CreateDbContext();
        CollectionAssert.AreEqual(new[] { "First", "Second", "Third" }, context.Parents.OrderBy(p => p.Id).Select(p => p.Name).ToArray());
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void Seed_overloads_should_add_the_given_data(int overload)
    {
        var parent = new Parent { Id = 1, Name = "Seed" };
        IEnumerable<object?> seedData = new object?[] { parent, null };
        ITestBuilder builder = overload switch
        {
            0 => new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>(seedData, Guid.NewGuid().ToString()),
            1 => new TestBuilder().With(new SqliteDatabaseTestConfiguration<SqliteTestDatabase>(parent, null)),
            2 => new TestBuilder().With(new SqliteDatabaseTestConfiguration<SqliteTestDatabase>(seedData)),
            3 => new TestBuilder().With(new SqliteDatabaseTestConfiguration<SqliteTestDatabase>(seedData, Guid.NewGuid().ToString())),
            _ => new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>(seedData)
        };
        using var environment = builder.Build();
        using var context = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();

        Assert.AreEqual("Seed", context.Parents.Single().Name);
    }

    [TestMethod]
    public void AddSeedData_should_support_objects_enumerables_and_params()
    {
        var config = new SqliteDatabaseTestConfiguration<SqliteTestDatabase>();
        config.AddSeedData((object)new Parent { Id = 1, Name = "First" });
        config.AddSeedData((IEnumerable<object?>)new object?[] { new Parent { Id = 2, Name = "Second" }, null });
        config.AddSeedData(new Parent { Id = 3, Name = "Third" }, null);
        config.AddSeedData((object?)null);
        config.AddSeedData((IEnumerable<object?>?)null);
        config.AddSeedData((object?[]?)null);
        using var environment = new TestBuilder().With(config).Build();
        using var context = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();

        Assert.AreEqual(3, context.Parents.Count());
    }

    [TestMethod]
    public void Data_should_persist_between_contexts()
    {
        using var environment = new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>().Build();
        var factory = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>();
        using (var context = factory.CreateDbContext())
        {
            context.Parents.Add(new Parent { Name = "Persisted" });
            context.SaveChanges();
        }
        using var nextContext = factory.CreateDbContext();

        Assert.AreEqual("Persisted", nextContext.Parents.Single().Name);
    }

    [TestMethod]
    public void Default_databases_should_be_isolated()
    {
        using var first = new TestBuilder()
            .WithSqliteDatabase<SqliteTestDatabase>(new Parent { Name = "First" })
            .Build();
        using var second = new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>().Build();
        using var firstContext = first.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();
        using var secondContext = second.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();

        Assert.AreEqual(1, firstContext.Parents.Count());
        Assert.AreEqual(0, secondContext.Parents.Count());
    }

    [TestMethod]
    public void Named_databases_should_share_data_until_all_environments_are_disposed()
    {
        var name = Guid.NewGuid().ToString();
        using var first = new TestBuilder()
            .WithSqliteDatabase<SqliteTestDatabase>(new object?[] { new Parent { Name = "Shared" } }, name)
            .Build();
        using var second = new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>(name).Build();
        first.Dispose();
        using (var context = second.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext())
        {
            Assert.AreEqual("Shared", context.Parents.Single().Name);
        }
        second.Dispose();
        using var third = new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>(name).Build();
        using var thirdContext = third.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();

        Assert.AreEqual(0, thirdContext.Parents.Count());
    }

    [TestMethod]
    public void Different_context_types_should_have_independent_databases()
    {
        using var environment = new TestBuilder()
            .WithSqliteDatabase<SqliteTestDatabase>(new Parent { Name = "First" })
            .WithSqliteDatabase<OtherSqliteTestDatabase>(new Parent { Name = "Second" })
            .Build();
        using var first = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();
        using var second = environment.GetRequiredService<IDbContextFactory<OtherSqliteTestDatabase>>().CreateDbContext();

        Assert.AreEqual("First", first.Parents.Single().Name);
        Assert.AreEqual("Second", second.Parents.Single().Name);
        Assert.AreNotSame(first.Database.GetDbConnection(), second.Database.GetDbConnection());
    }

    [TestMethod]
    public void Contexts_should_have_independent_connections_and_transactions()
    {
        using var environment = new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>().Build();
        var factory = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>();
        using var first = factory.CreateDbContext();
        using var second = factory.CreateDbContext();
        using var firstTransaction = first.Database.BeginTransaction();

        Assert.AreNotSame(first.Database.GetDbConnection(), second.Database.GetDbConnection());
        Assert.IsNull(second.Database.CurrentTransaction);
        first.Parents.Add(new Parent { Name = "Rolled back" });
        first.SaveChanges();
        firstTransaction.Rollback();
        using var secondTransaction = second.Database.BeginTransaction();
        second.Parents.Add(new Parent { Name = "Also rolled back" });
        second.SaveChanges();
        secondTransaction.Rollback();
        Assert.AreEqual(0, second.Parents.Count());
    }

    [TestMethod]
    public void Foreign_key_violations_should_fail()
    {
        using var environment = new TestBuilder().WithSqliteDatabase<SqliteTestDatabase>().Build();
        using var context = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();
        context.Children.Add(new Child { ParentId = 123 });

        var exception = Assert.ThrowsExactly<DbUpdateException>(() => context.SaveChanges());
        Assert.IsInstanceOfType<SqliteException>(exception.InnerException);
        Assert.AreEqual(19, ((SqliteException)exception.InnerException).SqliteErrorCode);
    }

    [TestMethod]
    public void Disposing_environment_should_delete_the_database()
    {
        using var environment = new TestBuilder()
            .WithSqliteDatabase<SqliteTestDatabase>(new Parent { Name = "Seed" })
            .Build();
        string connectionString;
        using (var context = environment.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext())
        {
            connectionString = context.Database.GetDbConnection().ConnectionString;
        }
        environment.Dispose();

        AssertDatabaseDeleted(connectionString);
    }

    [TestMethod]
    public void Failed_seeding_should_release_the_database_and_propagate_the_error()
    {
        string? connectionString = null;
        var builder = new TestBuilder()
            .WithAction((IServiceProvider provider) =>
            {
                using var context = provider.GetRequiredService<IDbContextFactory<SqliteTestDatabase>>().CreateDbContext();
                connectionString = context.Database.GetDbConnection().ConnectionString;
            })
            .WithSqliteDatabase<SqliteTestDatabase>(new Child { ParentId = 123 });

        Assert.ThrowsExactly<DbUpdateException>(() => builder.Build());
        Assert.IsNotNull(connectionString);
        AssertDatabaseDeleted(connectionString);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(":memory:")]
    public void Invalid_database_names_should_fail_explicitly(string name)
    {
        Assert.ThrowsExactly<ArgumentException>(() => new SqliteDatabaseTestConfiguration<SqliteTestDatabase>(name));
    }

    private static void AssertDatabaseDeleted(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table'";
        Assert.AreEqual(0L, command.ExecuteScalar());
    }

    public class SqliteTestDatabase : DbContext
    {
        public SqliteTestDatabase(DbContextOptions<SqliteTestDatabase> options) : base(options)
        {
        }

        public DbSet<Parent> Parents => Set<Parent>();
        public DbSet<Child> Children => Set<Child>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Child>().HasOne<Parent>().WithMany().HasForeignKey(c => c.ParentId);
        }
    }

    public class OtherSqliteTestDatabase : DbContext
    {
        public OtherSqliteTestDatabase(DbContextOptions<OtherSqliteTestDatabase> options) : base(options)
        {
        }

        public DbSet<Parent> Parents => Set<Parent>();
    }

    public class Parent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
    }
}
