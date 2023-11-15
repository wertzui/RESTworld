using ExampleBlog.Common.Enums;
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