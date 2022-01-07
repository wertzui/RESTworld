namespace RESTworld.AspNetCore.DependencyInjection
{
    /// <summary>
    /// Contains versioning options to be specified inside the <see cref="RestWorldOptions"/> in your appsettings.
    /// </summary>
    public class VersioningOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether versioning through a query parameter is allowed.
        /// This allows versioning not only through the Accept header, but also in a query parameter style (https://example.org/blog/42?v=2).
        /// The default is false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if versioning through query parameters should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool AllowQueryParameterVersioning { get; set; }

        /// <summary>
        /// Gets or sets the default version.
        /// Must be either a positive integer value, or the string "*".
        /// The default is "*" (always use the latest version).
        /// </summary>
        /// <value>
        /// The default version.
        /// </value>
        public string? DefaultVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter that is used for versioning.
        /// The default is "v".
        /// </summary>
        /// <value>
        /// The name of the parameter.
        /// </value>
        public string? ParameterName { get; set; }
    }
}