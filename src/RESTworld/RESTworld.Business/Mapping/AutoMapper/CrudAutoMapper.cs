using AutoMapper;

namespace RESTworld.Business.Mapping.AutoMapper;

/// <summary>
/// Provides mapping functionality between an entity and its associated Data Transfer Objects (DTOs) for create, read,
/// update, and query operations in a CRUD application.
/// </summary>
/// <remarks>
/// This class leverages AutoMapper to project and map entities and DTOs.
/// Ensure that your mappings are configured in the provided IMapper instance, including any necessary projections for queryable mappings.
/// </remarks>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TCreateDto">The DTO used when creating new entries.</typeparam>
/// <typeparam name="TQueryDto">The type used for OData query operations.</typeparam>
/// <typeparam name="TGetListDto">The result type when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The result type when getting a single entry.</typeparam>
/// <typeparam name="TUpdateDto">The DTO used when updating existing entries.</typeparam>
public class CrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto> :
    ReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>,
    ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
{
    /// <summary>
    /// Initializes a new instance of the CrudAutoMapper class using the specified object mapper.
    /// </summary>
    /// <param name="mapper">The object mapper used to configure mapping between source and destination types. Cannot be null.</param>
    public CrudAutoMapper(IMapper mapper)
        : base(mapper)
    {
    }

    /// <inheritdoc/>
    public TEntity MapCreateToEntity(TCreateDto createDto) => Mapper.Map<TEntity>(createDto);

    /// <inheritdoc/>
    public void MapUpdateToEntity(TUpdateDto updateDto, TEntity entity) => Mapper.Map(updateDto, entity);
}
