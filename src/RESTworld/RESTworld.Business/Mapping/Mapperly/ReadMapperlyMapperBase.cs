using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RESTworld.Business.Mapping.Mapperly;

/// <summary>
/// A base class for read-only mappers that use Mapperly for mapping between entities and data transfer objects (DTOs).
/// </summary>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TQueryDto">The type used for OData query operations.</typeparam>
/// <typeparam name="TGetListDto">The result type when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The result type when getting a single entry.</typeparam>
public abstract partial class ReadMapperlyMapperBase<TEntity, TQueryDto, TGetListDto, TGetFullDto> : IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>
{
    protected ReadMapperlyMapperBase()
    {
        MemberMappingNames = BuildMemberMappingNames(MapEntityToFullExpression());
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> MemberMappingNames { get; }

    /// <inheritdoc/>
    public abstract TGetFullDto MapEntityToFull(TEntity entity);

    /// <summary>
    /// Creates an expression that maps an entity of type TEntity to its corresponding full data transfer object (DTO) of type TGetFullDto.
    /// </summary>
    /// <remarks>
    /// This expression is automatically generated using the mapping provided in <see cref="MapEntityToFull(TEntity)"/>.
    /// It is then used to automatically create <see cref="MemberMappingNames"/> based on the mapping and thus proviging translation from database errors to the correct property names on the DTOs.
    /// </remarks>
    /// <returns>An expression representing the mapping from a TEntity instance to a TGetFullDto instance.</returns>
    public abstract Expression<Func<TEntity, TGetFullDto>> MapEntityToFullExpression();

    /// <summary>
    /// Maps the query result to a list DTO.
    /// Configure your mappings here and they will be automatically used in <see cref="MapQueryToListQueryable(IQueryable{TQueryDto})"/>
    /// </summary>
    public abstract TGetListDto MapQueryToList(TQueryDto queryDto);

    /// <inheritdoc/>
    public abstract IQueryable<TQueryDto> MapEntityToQuery(IQueryable<TEntity> entities);

    /// <inheritdoc/>
    public abstract IQueryable<TGetListDto> MapQueryToListQueryable(IQueryable<TQueryDto> queryDtos);

    /// <summary>
    /// Maps the query result to a full DTO.
    /// Configure your mappings here and they will be automatically used in <see cref="MapQueryToFullQueryable(IQueryable{TQueryDto})"/>
    /// </summary>
    public abstract TGetFullDto MapQueryToFull(TQueryDto entity);

    /// <inheritdoc/>
    public abstract IQueryable<TGetFullDto> MapQueryToFullQueryable(IQueryable<TQueryDto> entities);

    private static FrozenDictionary<string, string> BuildMemberMappingNames(Expression<Func<TEntity, TGetFullDto>> expression)
    {
        if (expression.Body is MemberInitExpression memberInit)
        {
            var sourceParam = expression.Parameters[0];
            var mappings = new Dictionary<string, string>();

            foreach (var binding in memberInit.Bindings)
            {
                if (binding is MemberAssignment assignment &&
                    assignment.Expression is MemberExpression memberExpr &&
                    memberExpr.Expression == sourceParam)
                {
                    mappings[memberExpr.Member.Name] = assignment.Member.Name;
                }
            }

            return mappings.ToFrozenDictionary();
        }

        return FrozenDictionary<string, string>.Empty;
    }
}
