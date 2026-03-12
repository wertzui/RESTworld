using System.Linq;

namespace RESTworld.Business.Mapping;

/// <summary>
/// Interface for mapping entities to DTOs for read operations.
/// </summary>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TQueryDto">The type used for OData query operations.</typeparam>
/// <typeparam name="TGetListDto">The result type when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The result type when getting a single entry.</typeparam>
public interface IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto> : IMappingMemberNameProvider<TEntity, TGetFullDto>
{
    /// <summary>
    /// Queryable mapping is done in 2 stages to support OData.
    /// <list type="number">
    ///     <item><c>MapGetQuery</c> is used to map the queryable before OData is applied. This is used to map the queryable to the correct type and to apply any necessary filters or joins.</item>
    ///     <item>MapGetListQueryable or MapGetFullQueryable is used to map the queryable after OData is applied. This is done so properties which should only be available for querying can be excluded in the final result, like navigation properties.</item>
    /// </list>
    /// </summary>
    /// <param name="entities">The entity set to be mapped.</param>
    public IQueryable<TQueryDto> MapEntityToQueryQueryable(IQueryable<TEntity> entities);

    /// <summary>
    /// Queryable mapping is done in 2 stages to support OData.
    /// <list type="number">
    ///     <item>MapGetQuery is used to map the queryable before OData is applied. This is used to map the queryable to the correct type and to apply any necessary filters or joins.</item>
    ///     <item><c>MapGetListQueryable or MapGetFullQueryable</c> is used to map the queryable after OData is applied. This is done so properties which should only be available for querying can be excluded in the final result, like navigation properties.</item>
    /// </list>
    /// </summary>
    /// <param name="queryDtos">The entity set to be mapped.</param>
    public IQueryable<TGetListDto> MapQueryToListQueryable(IQueryable<TQueryDto> queryDtos);

    /// <summary>
    /// Maps one entity to a full DTO.
    /// This is used for the GetFull endpoint and should include all properties that should be returned when getting a single entity.
    /// </summary>
    /// <param name="entity">The entity to be mapped.</param>
    TGetFullDto MapEntityToFull(TEntity entity);

    /// <summary>
    /// Queryable mapping is done in 2 stages to support OData.
    /// <list type="number">
    ///     <item>MapGetQuery is used to map the queryable before OData is applied. This is used to map the queryable to the correct type and to apply any necessary filters or joins.</item>
    ///     <item><c>MapGetListQueryable or MapGetFullQueryable</c> is used to map the queryable after OData is applied. This is done so properties which should only be available for querying can be excluded in the final result, like navigation properties.</item>
    /// </list>
    /// </summary>
    /// <param name="entities">The entity set to be mapped.</param>
    public IQueryable<TGetFullDto> MapQueryToFullQueryable(IQueryable<TQueryDto> entities);
}
