namespace RESTworld.Business.Mapping.Mapperly;

/// <summary>
/// Base class for Mapperly mappers that implement the ICrudMapper contract.
/// This class provides a structured way to define mapping logic for create, read, and update operations in a CRUD scenario.
/// </summary>
/// <remarks>
/// This class combines create, read, and update mapping contracts to support full CRUD mapping
/// scenarios. Implementations should ensure consistent mapping logic between entities and their associated DTOs.
/// </remarks>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TCreateDto">The DTO used when creating new entries.</typeparam>
/// <typeparam name="TQueryDto">The type used for OData query operations.</typeparam>
/// <typeparam name="TGetListDto">The result type when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The result type when getting a single entry.</typeparam>
/// <typeparam name="TUpdateDto">The DTO used when updating existing entries.</typeparam>
public abstract partial class CrudMapperlyMapperBase<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto> :
    ReadMapperlyMapperBase<TEntity, TQueryDto, TGetListDto, TGetFullDto>,
    ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
{
    /// <inheritdoc/>
    public abstract TEntity MapCreateToEntity(TCreateDto createDto);

    /// <inheritdoc/>
    public abstract void MapUpdateToEntity(TUpdateDto updateDto, TEntity entity);
}
