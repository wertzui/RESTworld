using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace RESTworld.Testing
{
    /// <summary>
    /// Contains extension methods related to <see cref="InMemoryDatabaseTestConfiguration{TContext}"/>.
    /// </summary>
    public static class InMemoryDatabaseTestBuilderExtensions
    {
        /// <summary>
        /// Adds an in memory database to your tests.
        /// </summary>
        /// <typeparam name="TContext">The type of the database.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="databaseName">
        /// The name of the database or null to automatically generate one.
        /// </param>
        /// <returns>A builder that can be used to add further seed data to the database.</returns>
        public static ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> WithInMemoryDatabase<TContext>(this ITestBuilder builder, string? databaseName = null)
            where TContext : DbContext
        {
            return builder.WithInMemoryDatabase<TContext>(null, databaseName);
        }

        /// <summary>
        /// Adds an in memory database to your tests.
        /// </summary>
        /// <typeparam name="TContext">The type of the database.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="seedData">Data to seed your database with.</param>
        /// <returns>A builder that can be used to add further seed data to the database.</returns>
        public static ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> WithInMemoryDatabase<TContext>(this ITestBuilder builder, params object?[]? seedData)
            where TContext : DbContext
        {
            return builder.WithInMemoryDatabase<TContext>(seedData, null);
        }

        /// <summary>
        /// Adds an in memory database to your tests.
        /// </summary>
        /// <typeparam name="TContext">The type of the database.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="seedData">Data to seed your database with.</param>
        /// <param name="databaseName">
        /// The name of the database or null to automatically generate one.
        /// </param>
        /// <returns>A builder that can be used to add further seed data to the database.</returns>
        public static ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> WithInMemoryDatabase<TContext>(this ITestBuilder builder, IEnumerable<object?>? seedData, string? databaseName = null)
            where TContext : DbContext
        {
            var config = new InMemoryDatabaseTestConfiguration<TContext>(seedData, databaseName);
            return builder.With(config);
        }

        /// <summary>
        /// Adds data to the database.
        /// </summary>
        /// <typeparam name="TContext">The type of the database.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="seedData">The data to add.</param>
        /// <returns>A builder that can be used to add further seed data to the database.</returns>
        public static ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> WithSeed<TContext>(this ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> builder, params object?[]? seedData)
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
        public static ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> WithSeed<TContext>(this ITestBuilderWithConfig<InMemoryDatabaseTestConfiguration<TContext>> builder, IEnumerable<object?>? seedData)
            where TContext : DbContext
        {
            builder.Config.AddSeedData(seedData);
            return builder;
        }
    }

    /// <summary>
    /// This test configuration can add an in memory database to your tests.
    /// </summary>
    public class InMemoryDatabaseTestConfiguration<TContext> : ITestConfiguration
        where TContext : DbContext
    {
        private readonly string _databaseName;
        private readonly ICollection<object> _seedData = new List<object>();

        /// <summary>
        /// Creates a new instance of the <see cref="InMemoryDatabaseTestConfiguration{TContext}"/> class.
        /// </summary>
        /// <param name="databaseName">
        /// The name of the database or null to automatically generate one.
        /// </param>
        public InMemoryDatabaseTestConfiguration(string? databaseName = null)
        {
            _databaseName = databaseName ?? Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InMemoryDatabaseTestConfiguration{TContext}"/> class.
        /// </summary>
        /// <param name="seedData">Data to seed your database with.</param>
        /// <param name="databaseName">
        /// The name of the database or null to automatically generate one.
        /// </param>
        public InMemoryDatabaseTestConfiguration(IEnumerable<object?>? seedData, string? databaseName = null)
            : this(databaseName)
        {
            AddSeedData(seedData);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InMemoryDatabaseTestConfiguration{TContext}"/> class.
        /// </summary>
        /// <param name="seedData">Data to seed your database with.</param>
        /// <remarks>When using this constructor, the name is always auto generated.</remarks>
        public InMemoryDatabaseTestConfiguration(params object?[]? seedData)
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
            if (data is not null)
            {
                foreach (var d in data)
                {
                    AddSeedData(d);
                }
            }
        }

        /// <inheritdoc/>
        public void AfterConfigureServices(IServiceProvider provider)
        {
            if (_seedData.Count > 0)
            {
                var factory = provider.GetRequiredService<IDbContextFactory<TContext>>();
                using var context = factory.CreateDbContext();
                context.AddRange(_seedData);
                context.SaveChanges();
            }
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<TContext>(options => options.UseInMemoryDatabase(_databaseName));
        }
    }
}