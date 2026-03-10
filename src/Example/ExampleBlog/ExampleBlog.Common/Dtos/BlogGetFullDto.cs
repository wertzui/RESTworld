using RESTworld.Common.Dtos;
using System.Collections.Generic;

namespace ExampleBlog.Common.Dtos;

public class BlogCreateDto
{
    public required string Name { get; set; }
}

public class BlogQueryDto : ChangeTrackingDtoBase
{
    public required string Name { get; set; }

    public ICollection<PostQueryDto> Posts { get; } = [];
}

public class BlogGetListDto : ChangeTrackingDtoBase
{
    public required string Name { get; set; }
}

public class BlogGetFullDto : ChangeTrackingDtoBase
{
    public required string Name { get; set; }
}

public class BlogUpdateDto : ConcurrentDtoBase
{
    public required string Name { get; set; }
}