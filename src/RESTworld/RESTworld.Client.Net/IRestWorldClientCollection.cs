namespace RESTworld.Client.Net;

/// <summary>
/// A collection of all available <see cref="IRestWorldClient"/>s.
/// </summary>
public interface IRestWorldClientCollection
{
    /// <summary>
    /// Returns the client with the given name or throws an exception if the client cannot be found.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <returns>The client with the given name.</returns>
    /// <exception cref="ArgumentException">The client with the given name cannot be found.</exception>
    IRestWorldClient GetClient(string name);

    /// <summary>
    /// Tries to get the client with the given name.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <param name="client">The client with the given name.</param>
    /// <returns>true if the client exists; false otherwise.</returns>
    bool TryGetClient(string name, out IRestWorldClient client);
}