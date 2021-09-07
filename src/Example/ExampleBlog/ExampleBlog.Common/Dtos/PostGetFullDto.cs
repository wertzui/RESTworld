using ExampleBlog.Common.Enums;
using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class PostGetFullDto : ChangeTrackingDtoBase
    {
        public long AuthorId { get; set; }
        public long BlogId { get; set; }
        public string Headline { get; set; }
        public PostState State { get; set; }
        public string Text { get; set; }
    }
}