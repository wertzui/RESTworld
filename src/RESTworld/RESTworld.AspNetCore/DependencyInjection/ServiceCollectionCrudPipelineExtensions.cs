using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RESTworld.AspNetCore.Controller;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/> and CRUD controllers and services.
/// </summary>
public static class ServiceCollectionCrudPipelineExtensions
{
    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and
    /// the <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddCrudPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services, ApiVersion? apiVersion = null, bool isDeprecated = false)
                where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    {
        services.AddScoped<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
        services.AddForeignKeyForFormTo<TGetListDto>();
        RestControllerFeatureProvider.AddCrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();
        services.Configure<MvcApiVersioningOptions>(options =>
        {
            var controllerConvention = options.Conventions.Controller<CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
            if (apiVersion is null)
            {
                controllerConvention.IsApiVersionNeutral();
            }
            else
            {
                if (isDeprecated)
                    controllerConvention.HasDeprecatedApiVersion(apiVersion);
                else
                    controllerConvention.HasApiVersion(apiVersion);
            }
        });

        return services;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>, the
    /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto,
    /// TUpdateDto}"/> and the <typeparamref name="TAuthorizationhandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TAuthorizationhandler">
    /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
    /// </param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        services.AddCrudPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(apiVersion, isDeprecated);
        services.AddCrudAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(configuration);

        return services;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and a
    /// custom <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/> implementation.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>(this IServiceCollection services)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TService : class, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        services.AddScoped<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TService>();
        RestControllerFeatureProvider.AddCrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();
        services.AddForeignKeyForFormTo<TGetListDto>();

        return services;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>, a
    /// custom <typeparamref name="TService"/> and the <typeparamref name="TAuthorizationhandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationhandler">
    /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddCrudPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TService : class, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        services.AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();
        services.AddCrudAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(configuration);

        return services;
    }
}