using ExampleBlog.Common.Enums;
using RESTworld.EntityFrameworkCore.Models;

namespace ExampleBlog.Data.Models
{
    public class Post : ChangeTrackingEntityBase
    {
        public string Headline { get; set; }
        public string Text { get; set; }
        public long BlogId { get; set; }
        public virtual Blog Blog { get; set; }
        public long AuthorId { get; set; }
        public virtual Author Author { get; set; }
        public PostState State { get; set; }
    }
}