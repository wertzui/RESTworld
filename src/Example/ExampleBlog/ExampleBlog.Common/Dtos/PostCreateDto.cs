using ExampleBlog.Common.Enums;
using HAL.Common.Binary;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExampleBlog.Common.Dtos
{
    public class PostCreateDto
    {
        [Display(Name = "Author")]
        public long AuthorId { get; set; }
        [Display(Name = "Blog")]
        public long BlogId { get; set; }
        [Required]
        public string Headline { get; set; } = default!;
        public PostState State { get; set; }
        [DataType(DataType.MultilineText)]
        [Required]
        public string Text { get; set; } = default!;
        [JsonIgnore]
        public virtual AuthorDto? Author { get; set; }
        [JsonIgnore]
        public virtual BlogDto? Blog { get; set; }
        [RestWorldImage(
            AspectRatio = 120.0 / 40.0,
            ContainWithinAspectRatio = true,
            MaintainAspectRatio = true,
            OnlyScaleDown = false,
            ResizeToHeight = 40,
            ResizeToWidth = 120
            )]
        public HalFile? Image { get; set; }
        [DataType(DataType.Upload)]
        public HalFile? Attachement { get; set; }
    }
}