using RESTworld.Common.Dtos;
using System.ComponentModel.DataAnnotations;

namespace ExampleBlog.Common.Dtos
{
    [DisplayColumn(nameof(LastName))]
    public class AuthorDto : ChangeTrackingDtoBase
    {
        [Display(Name = "E-Mail")]
        public string Email { get; set; }
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Display(Name = "Last name")]
        public string LastName { get; set; }
    }
}