using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;

namespace ExampleBlog.Data.Models
{
    public class Author : ChangeTrackingEntityBase
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}