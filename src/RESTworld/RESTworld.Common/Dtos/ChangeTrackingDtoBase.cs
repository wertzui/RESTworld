using System;
using System.ComponentModel.DataAnnotations;

namespace RESTworld.Common.Dtos
{
    /// <summary>
    /// A base class for DTOs which support tracking the creation and last change dates.
    /// </summary>
    public class ChangeTrackingDtoBase : DtoBase
    {
        /// <summary>
        /// Gets the date and time when this instance was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets the user who created this instance.
        /// </summary>
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets the date and time when this instance was last changed.
        /// </summary>
        public DateTimeOffset LastChangedAt { get; set; }

        /// <summary>
        /// Gets the user who changed this instance last.
        /// </summary>
        [MaxLength(100)]
        public string LastChangedBy { get; set; }
    }
}