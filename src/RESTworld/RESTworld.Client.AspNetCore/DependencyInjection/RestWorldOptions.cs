namespace RESTworld.Client.AspNetCore.DependencyInjection
{
    /// <summary>
    /// The same as <see cref="RESTworld.AspNetCore.DependencyInjection.RestWorldOptions"/> but with <see cref="ClientSettings"/>.
    /// </summary>
    public class RestWorldOptions : RESTworld.AspNetCore.DependencyInjection.RestWorldOptions
    {
        /// <summary>
        /// Settings for the client to discover backend APIs.
        /// </summary>
        public ClientSettings? ClientSettings { get; set; }
    }
}
