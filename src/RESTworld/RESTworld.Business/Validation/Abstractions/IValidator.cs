namespace RESTworld.Business.Validation.Abstractions;

/// <summary>
/// Validates the creation and update of an entity.
/// </summary>
/// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
/// <typeparam name="TUpdateDto">The type of the update DTO.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IValidator<TCreateDto, TUpdateDto, TEntity> : ICreateValidator<TCreateDto, TEntity>, IUpdateValidator<TUpdateDto, TEntity>
{
}