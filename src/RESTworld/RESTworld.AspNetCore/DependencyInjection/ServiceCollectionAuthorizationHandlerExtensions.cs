using Microsoft.Extensions.Configuration;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/> and authorization handlers.
/// </summary>
public static class ServiceCollectionAuthorizationHandlerExtensions
{
    /// <summary>
    /// Adds an <see cref="IBasicAuthorizationHandler{TResponse}"/> which you
    /// can use from your custom service method.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationHandler, TResponse>(this IServiceCollection services, IConfiguration configuration)
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TResponse>
    {
        if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
        {
            services.AddScoped<IBasicAuthorizationHandler<TResponse>, TAuthorizationHandler>();
            services.AddScoped<TAuthorizationHandler>();
        }

        return services;
    }

    /// <summary>
    /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
    /// can use when calling <see cref="ServiceBase.TryExecuteWithAuthorizationAsync{T1, TResponse, TAuthorizationHandler}(T1, System.Func{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Func{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Collections.Generic.IEnumerable{TAuthorizationHandler}, System.Threading.CancellationToken)"/> from your service method.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationHandler, TRequest, TResponse>(this IServiceCollection services, IConfiguration configuration)
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TRequest, TResponse>
    {
        if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
        {
            services.AddScoped<IBasicAuthorizationHandler<TRequest, TResponse>, TAuthorizationHandler>();
            services.AddScoped<TAuthorizationHandler>();
        }

        return services;
    }

    /// <summary>
    /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
    /// can use when calling <see cref="DbServiceBase{TContext}.TryExecuteWithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Collections.Generic.IEnumerable{TAuthorizationHandler}, System.Threading.CancellationToken)"/> from your service method.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationHandler, TEntity, TRequest, TResponse>(this IServiceCollection services, IConfiguration configuration)
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
        where TEntity : ConcurrentEntityBase
    {
        if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
        {
            services.AddScoped<IBasicAuthorizationHandler<TEntity, TRequest, TResponse>, TAuthorizationHandler>();
            services.AddScoped<TAuthorizationHandler>();
        }

        return services;
    }

    /// <summary>
    /// Adds an <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
    /// which will automatically be called by any <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a Query operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddCrudAuthorizationHandler<TAuthorizationHandler, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services, IConfiguration configuration)
        where TAuthorizationHandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    {
        if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
        {
            services.AddScoped<ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>, TAuthorizationHandler>();
            services.AddScoped<TAuthorizationHandler>();
        }

        return services;
    }

    /// <summary>
    /// Adds an <see cref="IReadAuthorizationHandler{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>
    /// which will automatically be called by any <see cref="ReadServiceBase{TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for queries of a List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddReadAuthorizationHandler<TAuthorizationHandler, TEntity, TQueryDto, TGetListDto, TGetFullDto>(this IServiceCollection services, IConfiguration configuration)
        where TAuthorizationHandler : class, IReadAuthorizationHandler<TEntity, TQueryDto, TGetListDto, TGetFullDto>
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
    {
        if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
        {
            services.AddScoped<IReadAuthorizationHandler<TEntity, TQueryDto, TGetListDto, TGetFullDto>, TAuthorizationHandler>();
            services.AddScoped<TAuthorizationHandler>();
        }

        return services;
    }
}