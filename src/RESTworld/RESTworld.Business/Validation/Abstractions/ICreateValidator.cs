using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation.Abstractions;

/// <summary>
/// Validates the creation of an entity.
/// </summary>
/// <typeparam name="TCreateDto">The type of the DTO.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface ICreateValidator<TCreateDto, TEntity>
{
    /// <summary>
    /// Validates the creation of an entity before it has been created with the values from the DTO.
    /// </summary>
    /// <param name="dto">The DTO containing the values to create the entity from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateBeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the creation of an entity after it has been created with the values from the DTO, but before it is saved.
    /// </summary>
    /// <param name="dto">The DTO containing the values to create the entity from.</param>
    /// <param name="entity">The entity that has been created, but not yet saved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateAfterCreateAsync(TCreateDto dto, TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the creation of an entity before it has been created with the values from the DTO.
    /// </summary>
    /// <param name="dtos">The DTOs containing the values to create the entity from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateCollectionBeforeCreateAsync(IEnumerable<TCreateDto> dtos, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the creation of an entity after it has been created with the values from the DTO, but before it is saved.
    /// </summary>
    /// <param name="dtosAndEntities">The DTOs and entities containing the values to create the entity from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The result of the validation.</returns>
    public Task<IValidationResults> ValidateCollectionAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken);
}
