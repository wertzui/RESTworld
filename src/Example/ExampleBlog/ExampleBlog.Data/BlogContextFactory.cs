using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExampleBlog.Data
{
    public class BlogContextFactory : IDesignTimeDbContextFactory<BlogDatabase>
    {
        public BlogDatabase CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogDatabase>();
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer();

            return new BlogDatabase(optionsBuilder.Options);
        }
    }
}