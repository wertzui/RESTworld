namespace RESTworld.Common.Dtos
{
    /// <summary>
    /// A base class for all DTOs which have once been stored in the database.
    /// </summary>
    public class DtoBase
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp which is used for concurrency detection.
        /// </summary>
        public virtual byte[] Timestamp { get; set; }
    }
}