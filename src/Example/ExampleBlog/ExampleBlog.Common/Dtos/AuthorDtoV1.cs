using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorDtoV1 : ChangeTrackingDtoBase
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}