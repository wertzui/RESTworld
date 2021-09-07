using System;
using System.ComponentModel.DataAnnotations;

namespace RESTworld.EntityFrameworkCore.Models
{
    /// <summary>
    /// Enhances the <see cref="EntityBase"/> class to provide some information about who and when an entity was created an last updated.
    /// These values are automatically set when <see cref="DbContextBase.SaveChangesAsync(bool, string, System.Threading.CancellationToken)"/> is called.
    /// </summary>
    /// <seealso cref="EntityBase" />
    public class ChangeTrackingEntityBase : EntityBase
    {
        /// <summary>
        /// Gets or sets the date and time when this entity was created.
        /// </summary>
        /// <value>
        /// The date and time when this entity was created.
        /// </value>
        public virtual DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created this entity.
        /// </summary>
        /// <value>
        /// The user who created this entity.
        /// </value>
        [MaxLength(100)]
        public virtual string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this entity was last changed.
        /// </summary>
        /// <value>
        /// The date and time when this entity was last changed.
        /// </value>
        public virtual DateTimeOffset LastChangedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who was the last to change this entity.
        /// </summary>
        /// <value>
        /// The user who was the last to change this entity.
        /// </value>
        [MaxLength(100)]
        public virtual string LastChangedBy { get; set; }
    }
}