namespace RESTworld.Client.AspNetCore.DependencyInjection
{
    /// <summary>
    /// An URL to a backend API that is used by the client to discover available APIs.
    /// </summary>
    public class ApiUrl
    {
        private string? _url;

        /// <summary>
        /// The name of the API.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The URL to the root of the API.
        /// </summary>
        public string? Url
        {
            get => _url;
            set => _url = (value?.EndsWith('/')).GetValueOrDefault() ? value : value + '/';
        }

        /// <summary>
        /// The version of the API.
        /// </summary>
        public int? Version { get; set; }
    }
}