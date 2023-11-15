using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation.Abstractions;

/// <summary>
/// Validates the update of an entity.
/// </summary>
/// <typeparam name="TUpdateDto">The type of the DTO.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IUpdateValidator<TUpdateDto, TEntity>
{
    /// <summary>
    /// Validates the update of an entity after it has been updated with the values from the DTO, but before it is saved.
    /// </summary>
    /// <param name="dto">The DTO containing the updated values.</param>
    /// <param name="entity">The entity that has been updated, but not yet saved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateAfterUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the update of an entity before it has been updated with the values from the DTO.
    /// </summary>
    /// <param name="dto">The DTO containing the updated values.</param>
    /// <param name="entity">The entity that has been nut yet updated.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateBeforeUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the update of an entity after it has been updated with the values from the DTO, but before it is saved.
    /// </summary>
    /// <param name="dtosAndEntities">The DTO and entities containing the updated values.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateCollectionAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the update of an entity before it has been updated with the values from the DTO.
    /// </summary>
    /// <param name="dtosAndEntities">The DTO and entities containing the updated values to update the entity with.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateCollectionBeforeUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken);
}
