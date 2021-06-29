using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorDto : ChangeTrackingDtoBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
