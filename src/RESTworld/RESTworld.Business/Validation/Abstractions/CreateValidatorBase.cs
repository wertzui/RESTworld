using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation.Abstractions
{
    /// <summary>
    /// A base class for CreateValidators that simply returns successful validation results for
    /// every validation.
    /// </summary>
    /// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class CreateValidatorBase<TCreateDto, TEntity> : ValidatorBase, ICreateValidator<TCreateDto, TEntity>
    {
        /// <inheritdoc/>
        public virtual Task<IValidationResults> ValidateAfterCreateAsync(TCreateDto dto, TEntity entity, CancellationToken cancellationToken)
            => CompletedSuccessfulValidation;

        /// <inheritdoc/>
        public virtual Task<IValidationResults> ValidateBeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken)
            => CompletedSuccessfulValidation;

        /// <inheritdoc/>
        public virtual Task<IValidationResults> ValidateCollectionAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
            => CompletedSuccessfulValidation;

        /// <inheritdoc/>
        public virtual Task<IValidationResults> ValidateCollectionBeforeCreateAsync(IEnumerable<TCreateDto> dtos, CancellationToken cancellationToken)
            => CompletedSuccessfulValidation;
    }
}