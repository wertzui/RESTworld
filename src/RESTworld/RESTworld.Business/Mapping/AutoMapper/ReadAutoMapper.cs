using AutoMapper;
using AutoMapper.Internal;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RESTworld.Business.Mapping.AutoMapper;

/// <summary>
/// Provides mapping functionality between entity types and their corresponding data transfer objects (DTOs) for read
/// operations, supporting both single and collection mappings.
/// </summary>
/// <remarks>
/// This class leverages AutoMapper to project and map entities and DTOs.
/// Ensure that your mappings are configured in the provided IMapper instance, including any necessary projections for queryable mappings.
/// </remarks>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TQueryDto">The type used for OData query operations.</typeparam>
/// <typeparam name="TGetListDto">The result type when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The result type when getting a single entry.</typeparam>
public class ReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto> : IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>
{
    private static readonly Expression<Func<TGetListDto, TGetListDto>> _listResultExpression = SelectExpressionBuilder.ExcludingSerializationIgnored<TGetListDto>();
    protected IMapper Mapper { get; }

    /// <summary>
    /// Initializes a new instance of the ReadAutoMapper class using the specified AutoMapper instance.
    /// </summary>
    /// <param name="mapper">
    /// The AutoMapper instance used to perform object-to-object mapping between the entity and DTO types.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if the provided mapper is null.</exception>
    public ReadAutoMapper(IMapper mapper)
    {
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        var entityType = typeof(TEntity);
        var dtoType = typeof(TGetFullDto);

        MemberMappingNames = Mapper.ConfigurationProvider.Internal().GetAllTypeMaps()
            .Where(m => m.SourceType == entityType && m.DestinationType == dtoType)
            .SelectMany(m => m.PropertyMaps)
            .Where(p => p.SourceMember is not null && p.DestinationMember is not null)
            .GroupBy(p => p.SourceMember.Name)
            .ToFrozenDictionary(g => g.Key, g => g.First().DestinationMember.Name);
    }

    /// <inheritdoc/>
    public TGetFullDto MapEntityToFull(TEntity entity) => Mapper.Map<TGetFullDto>(entity);

    /// <inheritdoc/>
    public TGetListDto MapQueryToList(TQueryDto entity) => Mapper.Map<TGetListDto>(entity);

    /// <inheritdoc/>
    public IQueryable<TGetListDto> MapQueryToListQueryable(IQueryable<TQueryDto> entities) => Mapper.ProjectTo<TGetListDto>(entities);

    /// <inheritdoc/>
    public TGetFullDto MapQueryToFull(TQueryDto entity) => Mapper.Map<TGetFullDto>(entity);

    /// <inheritdoc/>
    public IQueryable<TGetFullDto> MapQueryToFullQueryable(IQueryable<TQueryDto> entities) => Mapper.ProjectTo<TGetFullDto>(entities);

    /// <inheritdoc/>
    public IQueryable<TQueryDto> MapEntityToQueryQueryable(IQueryable<TEntity> entities) => Mapper.ProjectTo<TQueryDto>(entities);

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> MemberMappingNames { get; }
}
