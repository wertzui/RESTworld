namespace RESTworld.Business.Mapping;

/// <summary>
/// An Interface for mapping between an entity and its corresponding Data Transfer Objects (DTOs) for create,
/// read, update, and list operations in a CRUD workflow.
/// </summary>
/// <remarks>
/// This interface combines create, read, and update mapping contracts to support full CRUD mapping
/// scenarios. Implementations should ensure consistent mapping logic between entities and their associated DTOs.
/// </remarks>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TCreateDto">The DTO used when creating new entries.</typeparam>
/// <typeparam name="TQueryDto">The type used for OData query operations.</typeparam>
/// <typeparam name="TGetListDto">The result type when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The result type when getting a single entry.</typeparam>
/// <typeparam name="TUpdateDto">The DTO used when updating existing entries.</typeparam>
public interface ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto> :
    IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>,
    ICreateMapper<TEntity, TCreateDto>,
    IUpdateMapper<TEntity, TUpdateDto>
{
}
