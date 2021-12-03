namespace ExampleBlog.Common.Dtos
{
    public class PostWithAuthorDto : PostGetFullDto
    {
        public override AuthorDto? Author { get => base.Author; set => base.Author = value; }
    }
}