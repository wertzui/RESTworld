using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorDtoV1 : ChangeTrackingDtoBase
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
