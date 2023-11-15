using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation.Abstractions;

/// <summary>
/// A base class for Validators that simply returns successful validation results for every validation.
/// </summary>
/// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
/// <typeparam name="TUpdateDto">The type of the update DTO.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public abstract class ValidatorBase<TCreateDto, TUpdateDto, TEntity> : ValidatorBase, IValidator<TCreateDto, TUpdateDto, TEntity>
{
    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateAfterCreateAsync(TCreateDto dto, TEntity entity, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateAfterUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateBeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateBeforeUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateCollectionAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateCollectionAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateCollectionBeforeCreateAsync(IEnumerable<TCreateDto> dtos, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;

    /// <inheritdoc/>
    public virtual Task<IValidationResults> ValidateCollectionBeforeUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
        => CompletedSuccessfulValidation;
}

/// <summary>
/// Base class for the generic validators. It just holds a completed successful validation instance.
/// </summary>
public abstract class ValidatorBase
{
    /// <summary>
    /// A completed successful validation.
    /// </summary>
    protected static readonly Task<IValidationResults> CompletedSuccessfulValidation = Task.FromResult(SuccessfullValidationResults.Instance);
}