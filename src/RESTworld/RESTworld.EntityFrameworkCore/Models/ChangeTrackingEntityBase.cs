using System;
using System.ComponentModel.DataAnnotations;

namespace RESTworld.EntityFrameworkCore.Models
{
    public class ChangeTrackingEntityBase : EntityBase
    {
        public DateTimeOffset CreatedAt { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTimeOffset LastChangedAt { get; set; }

        [MaxLength(100)]
        public string LastChangedBy { get; set; }
    }
}