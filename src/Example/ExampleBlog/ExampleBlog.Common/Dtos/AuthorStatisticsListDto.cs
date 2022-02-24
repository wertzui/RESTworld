using RESTworld.Common.Dtos;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorStatisticsListDto : DtoBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}