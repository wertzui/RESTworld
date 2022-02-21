using System;
using System.ComponentModel.DataAnnotations;

namespace RESTworld.Common.Dtos
{
    /// <summary>
    /// A base class for DTOs which support tracking the creation and last change dates.
    /// </summary>
    public class ChangeTrackingDtoBase : ConcurrentDtoBase
    {
        /// <summary>
        /// Gets the date and time when this instance was created.
        /// </summary>
        [Display(Name = "Created at", Order = 10000)]
        [Editable(false)]
        public virtual DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Gets the user who created this instance.
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Created by", Order = 10001)]
        [Editable(false)]
        public virtual string? CreatedBy { get; set; }

        /// <summary>
        /// Gets the date and time when this instance was last changed.
        /// </summary>
        [Display(Name = "Last changed at", Order = 10002)]
        [Editable(false)]
        public virtual DateTimeOffset? LastChangedAt { get; set; }

        /// <summary>
        /// Gets the user who changed this instance last.
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Last changed by", Order = 10003)]
        [Editable(false)]
        public virtual string? LastChangedBy { get; set; }
    }
}