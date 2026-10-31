namespace RESTworld.Business.Mapping;

/// <summary>
/// Interface for mapping an update DTO to an entity. This is used in the Update pipeline.
/// </summary>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TUpdateDto">The DTO used when updating existing entries.</typeparam>
public interface IUpdateMapper<TEntity, TUpdateDto>
{
    /// <summary>
    /// Updates an existing entity with the values from the update DTO. This is used in the Update pipeline.
    /// </summary>
    /// <param name="updateDto">The DTO to update the entity with.</param>
    public void MapUpdateToEntity(TUpdateDto updateDto, TEntity entity);

    /// <summary>
    /// Maps an entity to an update DTO. This is used in the Patch pipeline to get the current state
    /// of the entity as an update DTO before the patch operations are applied to it.
    /// </summary>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The update DTO with the current values of the entity.</returns>
    public TUpdateDto MapEntityToUpdate(TEntity entity);
}
