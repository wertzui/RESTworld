using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation.Abstractions;

/// <summary>
/// A base class for UpdateValidators that simply returns successful validation results for
/// every validation.
/// </summary>
/// <typeparam name="TUpdateDto">The type of the Update DTO.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public abstract class UpdateValidatorBase<TUpdateDto, TEntity> : ValidatorBase, IUpdateValidator<TUpdateDto, TEntity>
{
    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateAfterUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateBeforeUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateCollectionAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateCollectionBeforeUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;
}