using System;
using System.Collections.Generic;

namespace RESTworld.Common.Client
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

        /// <summary>
        /// Can contain any data that is not present in every RESTworld solution.
        /// Normally this contains any data that needs to be transferred from the backend to the frontend during application startup, like any keys, settings, etc.
        /// </summary>
        public IDictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
    }
}
