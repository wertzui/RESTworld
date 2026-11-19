using RESTworld.Business.Mapping.AutoMapper;

namespace RESTworld.Testing;

/// <summary>
/// Contains extension methods related to mapping.
/// </summary>
public static class MapperTestConfigurationExtensions
{
    /// <summary>
    /// Adds a Mapper based on AutoMapper to the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static ITestBuilder WithCrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>(this ITestBuilder builder)
    {
        return builder.WithCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, CrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>>();
    }

    /// <summary>
    /// Adds a Mapper based on AutoMapper to the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static ITestBuilder WithReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>(this ITestBuilder builder)
    {
        return builder.WithReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, ReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>>();
    }
}