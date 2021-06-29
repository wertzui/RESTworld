using ExampleBlog.Common.Enums;
using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class PostUpdateDto : DtoBase
    {
        public string Headline { get; set; }
        public string Text { get; set; }
        public long BlogId { get; set; }
        public long AuthorId { get; set; }
        public PostState State { get; set; }
    }
}
