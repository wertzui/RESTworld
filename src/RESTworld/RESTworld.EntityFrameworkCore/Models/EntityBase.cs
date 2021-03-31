using System.ComponentModel.DataAnnotations;

namespace RESTworld.EntityFrameworkCore.Models
{
    public class EntityBase
    {
        public virtual long Id { get; set; }

        [Timestamp]
        public virtual byte[] Timestamp { get; set; }
    }
}