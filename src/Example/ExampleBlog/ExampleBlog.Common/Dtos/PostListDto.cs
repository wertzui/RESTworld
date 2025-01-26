using RESTworld.Common.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExampleBlog.Common.Dtos;

public class PostListDto : ChangeTrackingDtoBase
{
    public required string Headline { get; set; }

    [Display(Name = "Author")]
    public long AuthorId { get; set; }

    [Display(Name = "Blog")]
    public long BlogId { get; set; }

    [JsonIgnore]
    public virtual AuthorDto? Author { get; set; }

    [JsonIgnore]
    public virtual BlogDto? Blog { get; set; }
}