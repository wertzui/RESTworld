using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation.Abstractions
{
    /// <summary>
    /// Validates all create and update operations. Normally you do not have to implement this
    /// interface yourself. You just need to implement one of the other interfaces and register it
    /// in the DI container. The rest will be taken care of automatically.
    /// </summary>
    /// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the update DTO.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IValidationService<TCreateDto, TUpdateDto, TEntity>
    {
        /// <summary>
        /// Validates the creation of an entity after it has been created with the values from the
        /// DTO, but before it is saved against all registered
        /// <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dto">The DTO containing the values to create the entity from.</param>
        /// <param name="entity">The entity that has been created, but not yet saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllAfterCreateAsync(TCreateDto dto, TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the update of an entity after it has been updated with the values from the
        /// DTO, but before it is saved against all registered
        /// <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <param name="entity">The entity that has been updated, but not yet saved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllAfterUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the creation of an entity before it has been created with the values from the
        /// DTO against all registered <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dto">The DTO containing the values to create the entity from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllBeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the update of an entity before it has been updated with the values from the
        /// DTO against all registered <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <param name="entity">The entity that has been nut yet updated.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllBeforeUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the creation of an entity after it has been created with the values from the
        /// DTO, but before it is saved against all registered
        /// <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dtosAndEntities">
        /// The DTOs and entities containing the values to create the entity from.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllCollectionsAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the update of an entity after it has been updated with the values from the
        /// DTO, but before it is saved against all registered
        /// <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dtosAndEntities">The DTO and entities containing the updated values.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllCollectionsAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the creation of an entity before it has been created with the values from the
        /// DTO against all registered <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dtos">The DTOs containing the values to create the entities from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllCollectionsBeforeCreateAsync(IEnumerable<TCreateDto> dtos, CancellationToken cancellationToken);

        /// <summary>
        /// Validates the update of an entity before it has been updated with the values from the
        /// DTO against all registered <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> s.
        /// </summary>
        /// <param name="dtosAndEntities">
        /// The DTO and entities containing the updated values to update the entity with.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The result of the validation.</returns>
        public Task<IValidationResults> ValidateAllCollectionsBeforeUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken);
    }
}