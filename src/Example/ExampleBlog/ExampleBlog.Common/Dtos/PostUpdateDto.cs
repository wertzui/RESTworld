﻿using ExampleBlog.Common.Enums;
using HAL.Common.Binary;
using RESTworld.Common.Dtos;
using System.ComponentModel.DataAnnotations;

namespace ExampleBlog.Common.Dtos
{
    public class PostUpdateDto : DtoBase
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
        [DataType(DataType.ImageUrl)]
        public HalFile Image { get; set; }
        [DataType(DataType.Upload)]
        public HalFile Attachement { get; set; }
    }
}