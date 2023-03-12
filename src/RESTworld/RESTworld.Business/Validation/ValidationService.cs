using RESTworld.Business.Validation.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation
{
    /// <inheritdoc/>
    public class ValidationService<TCreateDto, TUpdateDto, TEntity> : IValidationService<TCreateDto, TUpdateDto, TEntity>
    {
        private readonly IEnumerable<ICreateValidator<TCreateDto, TEntity>> _createValidators;
        private readonly bool _hasCreateValidators;
        private readonly bool _hasUpdateValidators;
        private readonly IEnumerable<IUpdateValidator<TUpdateDto, TEntity>> _updateValidators;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ValidationService{TCreateDto, TUpdateDto, TEntity}"/> class.
        /// </summary>
        /// <param name="createValidators">
        /// All registered <see cref="ICreateValidator{TCreateDto, TEntity}"/> s.
        /// </param>
        /// <param name="updateValidators">
        /// All registered <see cref="IUpdateValidator{TUpdateDto, TEntity}"/> s.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public ValidationService(
            IEnumerable<ICreateValidator<TCreateDto, TEntity>> createValidators,
            IEnumerable<IUpdateValidator<TUpdateDto, TEntity>> updateValidators)
        {
            _createValidators = createValidators ?? throw new ArgumentNullException(nameof(createValidators));
            _updateValidators = updateValidators ?? throw new ArgumentNullException(nameof(updateValidators));

            _hasCreateValidators = _createValidators.Any();
            _hasUpdateValidators = _updateValidators.Any();
        }

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllAfterCreateAsync(TCreateDto dto, TEntity entity, CancellationToken cancellationToken)
            => ValidateAllAsync(_createValidators, _hasCreateValidators, v => v.ValidateAfterCreateAsync(dto, entity, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllAfterUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken)
            => ValidateAllAsync(_updateValidators, _hasUpdateValidators, v => v.ValidateAfterUpdateAsync(dto, entity, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllBeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken)
            => ValidateAllAsync(_createValidators, _hasCreateValidators, v => v.ValidateBeforeCreateAsync(dto, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllBeforeUpdateAsync(TUpdateDto dto, TEntity entity, CancellationToken cancellationToken)
            => ValidateAllAsync(_updateValidators, _hasUpdateValidators, v => v.ValidateBeforeUpdateAsync(dto, entity, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllCollectionsAfterCreateAsync(IEnumerable<(TCreateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
            => ValidateAllAsync(_createValidators, _hasCreateValidators, v => v.ValidateCollectionAfterCreateAsync(dtosAndEntities, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllCollectionsAfterUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
            => ValidateAllAsync(_updateValidators, _hasUpdateValidators, v => v.ValidateCollectionAfterUpdateAsync(dtosAndEntities, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllCollectionsBeforeCreateAsync(IEnumerable<TCreateDto> dtos, CancellationToken cancellationToken)
            => ValidateAllAsync(_createValidators, _hasCreateValidators, v => v.ValidateCollectionBeforeCreateAsync(dtos, cancellationToken));

        /// <inheritdoc/>
        public Task<IValidationResults> ValidateAllCollectionsBeforeUpdateAsync(IEnumerable<(TUpdateDto dto, TEntity entity)> dtosAndEntities, CancellationToken cancellationToken)
            => ValidateAllAsync(_updateValidators, _hasUpdateValidators, v => v.ValidateCollectionBeforeUpdateAsync(dtosAndEntities, cancellationToken));

        private static async Task<IValidationResults> ValidateAllAsync<TValidator>(IEnumerable<TValidator> validators, bool hasValidators, Func<TValidator, Task<IValidationResults>> validateAsync)
        {
            if (!hasValidators)
                return SuccessfullValidationResults.Instance;

            var validationResults = new ValidationResults();

            try
            {
                var results = await Task.WhenAll(validators.Select(v => validateAsync(v)));

                foreach (var result in results)
                    validationResults.AddValidationFailures(result);

                return validationResults;
            }
            catch (Exception e)
            {
                throw CouldNotExecuteValidationException.Create<TCreateDto, TUpdateDto, TEntity>(e);
            }
        }
    }
}