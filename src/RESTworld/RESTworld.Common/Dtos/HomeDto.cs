namespace RESTworld.Common.Dtos
{
    /// <summary>
    /// This is displayed on the Home endpoint.
    /// </summary>
    public record HomeDto()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeDto"/> class.
        /// </summary>
        /// <param name="versions">Information about supported versions.</param>
        public HomeDto(VersionInformationDto? versions)
            : this()
        {
            Versions = versions;
        }

        /// <summary>
        /// Information about supported versions.
        /// </summary>
        public VersionInformationDto? Versions { get; set; }
    }
}