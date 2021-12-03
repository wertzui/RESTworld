using System;

namespace RESTworld.Client.AspNetCore.DependencyInjection
{
    /// <summary>
    /// Contains the settings which the client requests to discover backend APIs.
    /// </summary>
    public class ClientSettings
    {
        /// <summary>
        /// A collection of all backend available APIs which the client may use.
        /// </summary>
        public ApiUrl[] ApiUrls { get; set; } = Array.Empty<ApiUrl>();
    }
}
