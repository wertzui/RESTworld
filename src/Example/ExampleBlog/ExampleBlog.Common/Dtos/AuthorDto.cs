using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorDto : ChangeTrackingDtoBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
