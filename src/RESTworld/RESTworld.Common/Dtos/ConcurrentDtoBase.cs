namespace RESTworld.Common.Dtos
{
    /// <summary>
    /// A base class for all DTOs which have once been stored in the database and may be updated.
    /// </summary>
    public class ConcurrentDtoBase : DtoBase
    {
        /// <summary>
        /// Gets or sets the timestamp which is used for concurrency detection.
        /// </summary>
        public virtual byte[]? Timestamp { get; set; }
    }
}