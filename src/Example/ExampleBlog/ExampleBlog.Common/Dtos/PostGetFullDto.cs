using ExampleBlog.Common.Enums;
using HAL.Common.Binary;
using RESTworld.Common.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExampleBlog.Common.Dtos
{
    public class PostGetFullDto : ChangeTrackingDtoBase
    {
        [Display(Name = "Author")]
        public long AuthorId { get; set; }
        [Display(Name = "Blog")]
        public long BlogId { get; set; }
        [Required]
        public string Headline { get; set; }
        public PostState State { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        [JsonIgnore]
        public AuthorDto Author { get; set; }
        [JsonIgnore]
        public BlogDto Blog { get; set; }
        [DataType(DataType.ImageUrl)]
        public HalFile Image { get; set; }
        [DataType(DataType.Upload)]
        public HalFile Attachement { get; set; }
    }
}