namespace ExampleBlog.Common.Dtos
{
    public class PostWithAuthorDto : PostGetFullDto
    {
        public AuthorDto Author { get; set; }
    }
}