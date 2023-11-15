namespace RESTworld.Common.Client;

/// <summary>
/// The same as <see cref="RestWorldClientOptions"/> but with <see cref="ClientSettings"/>.
/// </summary>
public class RestWorldClientOptions
{
    /// <summary>
    /// Settings for the client to discover backend APIs.
    /// </summary>
    public ClientSettings? ClientSettings { get; set; }
}
