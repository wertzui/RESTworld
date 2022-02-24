using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;

namespace ExampleBlog.Common.Dtos
{
    public class AuthorStatisticsFullDto : DtoBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IDictionary<DateTimeOffset, long> PostsPerMonth { get; set; }
        public IDictionary<DateTimeOffset, long> PostsPerYear { get; set; }
        public long TotalPosts { get; set; }
    }
}