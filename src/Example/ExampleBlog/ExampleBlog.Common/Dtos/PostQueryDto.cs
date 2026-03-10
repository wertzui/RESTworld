using ExampleBlog.Common.Enums;
using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos;

public class PostQueryDto : ChangeTrackingDtoBase
{
    public long AuthorId { get; set; }

    public long BlogId { get; set; }

    public string Headline { get; set; } = default!;

    public PostState State { get; set; }

    public string Text { get; set; } = default!;

    public virtual AuthorDto? Author { get; set; }

    public virtual BlogGetFullDto? Blog { get; set; }
}