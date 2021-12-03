using ExampleBlog.Common.Enums;
using HAL.Common.Binary;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExampleBlog.Common.Dtos
{
    // We do not inherit from PostGetFullDto here as that DTO has the Author property defined as AuthorDto and we need AuthorDtoV1
    public class PostWithAuthorDtoV1
    {
        [Display(Name = "Author")]
        public long AuthorId { get; set; }
        [Display(Name = "Blog")]
        public long BlogId { get; set; }
        [Required]
        public string Headline { get; set; } = default!;
        public PostState State { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; } = default!;
        // No JsonIgnore here, because in this custom DTO we want to render the Author inline.
        public virtual AuthorDtoV1? Author { get; set; }
        [JsonIgnore]
        public virtual BlogDto? Blog { get; set; }
        [DataType(DataType.ImageUrl)]
        public HalFile? Image { get; set; }
        [DataType(DataType.Upload)]
        public HalFile? Attachement { get; set; }
    }
}