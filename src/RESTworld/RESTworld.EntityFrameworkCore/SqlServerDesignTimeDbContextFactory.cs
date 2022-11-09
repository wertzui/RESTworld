using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace RESTworld.EntityFrameworkCore
{
    /// <summary>
    /// A simple implementation for <see cref="IDesignTimeDbContextFactory{TContext}"/> for SQL backed DbContext instances.
    /// Saves you from having to manually implement the interface over and over.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public class SqlServerDesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext
    {
        /// <inheritdoc/>
        public TDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseSqlServer();
            var context = Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options) as TDbContext;
            if (context is null)
                throw new Exception($"Unable to create an instance of {typeof(TDbContext).Name}");

            return context;
        }
    }
}