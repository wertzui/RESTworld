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
/// Contains extension methods for <see cref="IServiceCollection"/> and Read controllers and services.
/// </summary>
public static class ServiceCollectionReadPipelineExtensions
{
    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/> and
    /// the <see cref="ReadServiceBase{TContext, TEntity, TGetListDto, TGetFullDto}"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddReadPipeline<TContext, TEntity, TGetListDto, TGetFullDto>(this IServiceCollection services, ApiVersion? apiVersion = null, bool isDeprecated = false)
                where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

    {
        services.AddScoped<IReadServiceBase<TEntity, TGetListDto, TGetFullDto>, ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>>();
        services.AddForeignKeyForFormTo<TGetListDto>();
        RestControllerFeatureProvider.AddReadController<TEntity, TGetListDto, TGetFullDto>();
        services.Configure<MvcApiVersioningOptions>(options =>
        {
            var controllerConvention = options.Conventions.Controller<ReadController<TEntity, TGetListDto, TGetFullDto>>();
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
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/>, the
    /// <see cref="ReadServiceBase{TContext, TEntity, TGetListDto, TGetFullDto}"/> and the <typeparamref name="TAuthorizationhandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TAuthorizationhandler">
    /// The type of the <see cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}"/>.
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
    public static IServiceCollection AddReadPipelineWithAuthorization<TContext, TEntity, TGetListDto, TGetFullDto, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TAuthorizationhandler : class, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
    {
        services.AddReadPipeline<TContext, TEntity, TGetListDto, TGetFullDto>(apiVersion, isDeprecated);
        services.AddReadAuthorizationHandler<TAuthorizationhandler, TEntity, TGetListDto, TGetFullDto>(configuration);

        return services;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/> and a
    /// custom <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="IReadServiceBase{TEntity, TGetListDto, TGetFullDto}"/> implementation.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddReadPipelineWithCustomService<TContext, TEntity, TGetListDto, TGetFullDto, TService>(this IServiceCollection services)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TService : class, IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
    {
        services.AddScoped<IReadServiceBase<TEntity, TGetListDto, TGetFullDto>, TService>();
        RestControllerFeatureProvider.AddReadController<TEntity, TGetListDto, TGetFullDto>();
        services.AddForeignKeyForFormTo<TGetListDto>();

        return services;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/>, a
    /// custom <typeparamref name="TService"/> and the <typeparamref name="TAuthorizationhandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="IReadServiceBase{TEntity, TGetListDto, TGetFullDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationhandler">
    /// The type of the <see cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}"/>.
    /// </typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddReadPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TGetListDto, TGetFullDto, TService, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TService : class, IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
        where TAuthorizationhandler : class, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
    {
        services.AddReadPipelineWithCustomService<TContext, TEntity, TGetListDto, TGetFullDto, TService>();
        services.AddReadAuthorizationHandler<TAuthorizationhandler, TEntity, TGetListDto, TGetFullDto>(configuration);

        return services;
    }
}