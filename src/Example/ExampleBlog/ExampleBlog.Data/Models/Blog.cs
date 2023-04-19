using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;

namespace ExampleBlog.Data.Models
{
    public class Blog : ChangeTrackingEntityBase
    {
        public required string Name { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}