using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos;

public class PostListDto : ChangeTrackingDtoBase
{
    public string Headline { get; set; } = default!;
}