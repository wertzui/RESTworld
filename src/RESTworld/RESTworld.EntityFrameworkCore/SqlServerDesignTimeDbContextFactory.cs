using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace RESTworld.EntityFrameworkCore
{
    internal class SqlServerDesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext
    {
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