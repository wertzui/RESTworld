using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace RESTworld.Testing;

/// <summary>
/// Contains extension methods related to <see cref="SqliteDatabaseTestConfiguration{TContext}"/>.
/// </summary>
public static class SqliteDatabaseTestBuilderExtensions
{
    /// <summary>
    /// Adds an in-memory SQLite database to your tests.
    /// </summary>
    /// <typeparam name="TContext">The type of the database.</typeparam>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <param name="databaseName">The name of the database or null to automatically generate one.</param>
    /// <returns>A builder that can be used to add further seed data to the database.</returns>
    public static ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> WithSqliteDatabase<TContext>(this ITestBuilder builder, string? databaseName = null)
        where TContext : DbContext
    {
        return builder.WithSqliteDatabase<TContext>(null, databaseName);
    }

    /// <summary>
    /// Adds an in-memory SQLite database to your tests.
    /// </summary>
    /// <typeparam name="TContext">The type of the database.</typeparam>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <param name="seedData">Data to seed your database with.</param>
    /// <returns>A builder that can be used to add further seed data to the database.</returns>
    public static ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> WithSqliteDatabase<TContext>(this ITestBuilder builder, params object?[]? seedData)
        where TContext : DbContext
    {
        return builder.WithSqliteDatabase<TContext>(seedData, null);
    }

    /// <summary>
    /// Adds an in-memory SQLite database to your tests.
    /// </summary>
    /// <typeparam name="TContext">The type of the database.</typeparam>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <param name="seedData">Data to seed your database with.</param>
    /// <param name="databaseName">The name of the database or null to automatically generate one.</param>
    /// <returns>A builder that can be used to add further seed data to the database.</returns>
    public static ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> WithSqliteDatabase<TContext>(this ITestBuilder builder, IEnumerable<object?>? seedData, string? databaseName = null)
        where TContext : DbContext
    {
        var config = new SqliteDatabaseTestConfiguration<TContext>(seedData, databaseName);
        return builder.With(config);
    }

    /// <summary>
    /// Adds data to the database.
    /// </summary>
    /// <typeparam name="TContext">The type of the database.</typeparam>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <param name="seedData">The data to add.</param>
    /// <returns>A builder that can be used to add further seed data to the database.</returns>
    public static ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> WithSeed<TContext>(this ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> builder, params object?[]? seedData)
        where TContext : DbContext
    {
        builder.Config.AddSeedData(seedData);
        return builder;
    }

    /// <summary>
    /// Adds data to the database.
    /// </summary>
    /// <typeparam name="TContext">The type of the database.</typeparam>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <param name="seedData">The data to add.</param>
    /// <returns>A builder that can be used to add further seed data to the database.</returns>
    public static ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> WithSeed<TContext>(this ITestBuilderWithConfig<SqliteDatabaseTestConfiguration<TContext>> builder, IEnumerable<object?>? seedData)
        where TContext : DbContext
    {
        builder.Config.AddSeedData(seedData);
        return builder;
    }
}

/// <summary>
/// This test configuration can add an in-memory SQLite database to your tests.
/// </summary>
/// <remarks>
/// The schema is created automatically. Databases with the same name share data while at least
/// one environment remains alive. Dispose contexts and environments to release the database.
/// </remarks>
public class SqliteDatabaseTestConfiguration<TContext> : ITestConfiguration
    where TContext : DbContext
{
    private readonly string _connectionString;
    private readonly ICollection<object> _seedData = new List<object>();

    /// <summary>
    /// Creates a new instance of the <see cref="SqliteDatabaseTestConfiguration{TContext}"/> class.
    /// </summary>
    /// <param name="databaseName">The name of the database or null to automatically generate one.</param>
    /// <exception cref="ArgumentException">The name is empty, whitespace or the reserved name <c>:memory:</c>.</exception>
    public SqliteDatabaseTestConfiguration(string? databaseName = null)
    {
        var name = databaseName ?? Guid.NewGuid().ToString();
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(databaseName));
        if (name == ":memory:")
            throw new ArgumentException("Use null for an automatically named in-memory SQLite database instead of :memory:.", nameof(databaseName));

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = name,
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
            Pooling = false,
            ForeignKeys = true
        }.ToString();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SqliteDatabaseTestConfiguration{TContext}"/> class.
    /// </summary>
    /// <param name="seedData">Data to seed your database with.</param>
    /// <param name="databaseName">The name of the database or null to automatically generate one.</param>
    public SqliteDatabaseTestConfiguration(IEnumerable<object?>? seedData, string? databaseName = null)
        : this(databaseName)
    {
        AddSeedData(seedData);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SqliteDatabaseTestConfiguration{TContext}"/> class.
    /// </summary>
    /// <param name="seedData">Data to seed your database with.</param>
    /// <remarks>When using this constructor, the name is always auto generated.</remarks>
    public SqliteDatabaseTestConfiguration(params object?[]? seedData)
        : this((IEnumerable<object?>?)seedData)
    {
    }

    /// <summary>
    /// Adds data to the database.
    /// </summary>
    /// <param name="data">The data to add.</param>
    public void AddSeedData(object? data)
    {
        if (data is not null)
            _seedData.Add(data);
    }

    /// <summary>
    /// Adds data to the database.
    /// </summary>
    /// <param name="data">The data to add.</param>
    public void AddSeedData(IEnumerable<object?>? data)
    {
        if (data is not null)
        {
            foreach (var d in data)
            {
                AddSeedData(d);
            }
        }
    }

    /// <summary>
    /// Adds data to the database.
    /// </summary>
    /// <param name="data">The data to add.</param>
    public void AddSeedData(params object?[]? data)
    {
        AddSeedData((IEnumerable<object?>?)data);
    }

    /// <inheritdoc/>
    public void AfterConfigureServices(IServiceProvider provider)
    {
        var factory = provider.GetRequiredService<IDbContextFactory<TContext>>();
        using var context = factory.CreateDbContext();
        context.Database.EnsureCreated();
        if (_seedData.Count > 0)
        {
            context.AddRange(_seedData);
            context.SaveChanges();
        }
    }

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddKeyedSingleton<SqliteConnection>(typeof(TContext), (_, _) => new SqliteConnection(_connectionString));
        services.AddDbContextFactory<TContext>((provider, options) =>
        {
            // Keep the database alive, but give each context its own connection for independent transactions.
            var connection = provider.GetRequiredKeyedService<SqliteConnection>(typeof(TContext));
            connection.Open();
            options.UseSqlite(_connectionString);
        });
    }
}
