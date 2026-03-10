namespace RESTworld.Business.Mapping;

/// <summary>
/// Interface for mapping a create DTO to an entity. This is used in the Create pipeline.
/// </summary>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TCreateDto">The DTO used when creating new entries.</typeparam>
public interface ICreateMapper<TEntity, TCreateDto>
{
    /// <summary>
    /// Maps a create DTO to an entity. This is used in the Create pipeline.
    /// </summary>
    /// <param name="createDto">The DTO to create a new entity from.</param>
    /// <returns>The mapped Entity.</returns>
    public TEntity MapCreateToEntity(TCreateDto createDto);
}
