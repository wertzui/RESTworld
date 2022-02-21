using System.ComponentModel.DataAnnotations;

namespace RESTworld.EntityFrameworkCore.Models
{
    /// <summary>
    /// A base class for all database entities (tables) which will provide an ID and a timestamp for concurrency detection.
    /// </summary>
    public class ConcurrentEntityBase : EntityBase
    {
        /// <summary>
        /// Gets or sets the timestamp which is used for concurrency detection.
        /// Set it to the value that you received from your DTO during a POST, PUT or DELETE request (This is automatically done when you are using the default REST pipeline).
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [Timestamp]
        public virtual byte[]? Timestamp { get; set; }
    }
}