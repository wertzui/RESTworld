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
            var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options);
            return context;
        }
    }
}