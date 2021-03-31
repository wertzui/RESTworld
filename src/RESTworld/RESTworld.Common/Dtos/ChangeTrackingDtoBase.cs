using System;
using System.ComponentModel.DataAnnotations;

namespace RESTworld.Common.Dtos
{
    public class ChangeTrackingDtoBase : DtoBase
    {
        public DateTimeOffset CreatedAt { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTimeOffset LastChangedAt { get; set; }

        [MaxLength(100)]
        public string LastChangedBy { get; set; }
    }
}