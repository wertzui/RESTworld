using ExampleBlog.Common.Enums;

namespace ExampleBlog.Common.Dtos
{
    public class PostCreateDto
    {
        public long AuthorId { get; set; }
        public long BlogId { get; set; }
        public string Headline { get; set; }
        public PostState State { get; set; }
        public string Text { get; set; }
    }
}