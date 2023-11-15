namespace RESTworld.EntityFrameworkCore.Models;

/// <summary>
/// A base class for all database entities (tables) which will provide an ID.
/// </summary>
public class EntityBase
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public virtual long Id { get; set; }
}