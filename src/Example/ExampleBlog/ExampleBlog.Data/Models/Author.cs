using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;

namespace ExampleBlog.Data.Models
{
    public class Author : ChangeTrackingEntityBase
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}