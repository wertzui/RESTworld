using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorStatisticsListDto : DtoBase
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}