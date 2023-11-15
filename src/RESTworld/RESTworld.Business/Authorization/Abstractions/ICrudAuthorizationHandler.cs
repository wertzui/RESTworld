using RESTworld.Business.Services.Abstractions;

namespace RESTworld.Business.Authorization.Abstractions;

/// <summary>
/// Handles authorizations for the complete list of CRUD operations.
/// Is used in <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s.
/// Each 'Handle...RequestAsync' method is called before the corresponding method is executed on the database.
/// Each 'Handle...ResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
/// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the direction they have been registered.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TCreateDto">The type of the DTO which is used on creation.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO which is used when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO which is used when getting a full record.</typeparam>
/// <typeparam name="TUpdateDto">The type of the DTO which is used for updating.</typeparam>
public interface ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> :
    ICreateAuthorizationHandler<TEntity, TCreateDto, TGetFullDto>,
    IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>,
    IUpdateAuthorizationHandler<TEntity, TUpdateDto, TGetFullDto>,
    IDeleteAuthorizationHandler<TEntity>
{
}