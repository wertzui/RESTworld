using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;

namespace ExampleBlog.Common.Dtos;

public class AuthorStatisticsFullDto : DtoBase
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required IDictionary<DateTimeOffset, long> PostsPerMonth { get; set; }
    public required IDictionary<DateTimeOffset, long> PostsPerYear { get; set; }
    public long TotalPosts { get; set; }
}