using ExampleBlog.Common.Enums;
using ExampleBlog.Data.Models;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // These will just fill in some example data so you have something to query on your first start.

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasData(
                new Blog { Id = 1, Name = "My first Blog" }.AddDefaults(),
                new Blog { Id = 2, Name = "My second Blog" }.AddDefaults(),
                new Blog { Id = 3, Name = "My third Blog" }.AddDefaults());
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasData(
                new Author { Id = 1, FirstName = "Jon", LastName = "Doe", Email = "jondoe@example.com" }.AddDefaults(),
                new Author { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "janedoe@example.com" }.AddDefaults());
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable(b => b.IsTemporal());

            var id = 1L;
            for (var blogId = 1; blogId <= 3; blogId++)
            {
                for (var authorId = 1; authorId <= 2; authorId++)
                {
                    for (int postNumber = 1; postNumber <= 42; postNumber++)
                    {
                        entity.HasData(new Post
                        {
                            Id = id++,
                            BlogId = blogId,
                            AuthorId = authorId,
                            Headline = $"Post number {postNumber}",
                            State = postNumber <= 33 ? PostState.Published : PostState.Draft,
                            Text = $"This is the post number {postNumber} in the blog {blogId} from author {authorId}."
                        }.AddDefaults());
                    }
                }
            }
        });
    }
}