using ExampleBlog.Common.Enums;

namespace ExampleBlog.Common.Dtos
{
    public class PostCreateDto
    {
        public string Headline { get; set; }
        public string Text { get; set; }
        public long BlogId { get; set; }
        public long AuthorId { get; set; }
        public PostState State { get; set; }
    }
}
