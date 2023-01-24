using Microsoft.Extensions.Configuration;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/> and authorization handlers.
    /// </summary>
    public static class ServiceCollectionAuthorizationHandlerExtensions
    {
        /// <summary>
        /// Adds an <see cref="IBasicAuthorizationHandler{TResponse}"/> which you
        /// can use from your custom service method.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationhandler, TResponse>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, IBasicAuthorizationHandler<TResponse>
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<IBasicAuthorizationHandler<TResponse>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }

        /// <summary>
        /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
        /// can use when calling <see cref="ServiceBase.TryExecuteWithAuthorizationAsync{T1, TResponse, TAuthorizationHandler}(T1, System.Func{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Func{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}, TAuthorizationHandler, System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse}, TAuthorizationHandler, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Collections.Generic.IEnumerable{TAuthorizationHandler})"/> from your service method.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationhandler, TRequest, TResponse>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, IBasicAuthorizationHandler<TRequest, TResponse>
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<IBasicAuthorizationHandler<TRequest, TResponse>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }

        /// <summary>
        /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
        /// can use when calling <see cref="DbServiceBase{TContext}.TryExecuteWithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}, TAuthorizationHandler, System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse}, TAuthorizationHandler, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Collections.Generic.IEnumerable{TAuthorizationHandler})"/> from your service method.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationhandler, TEntity, TRequest, TResponse>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
            where TEntity : ConcurrentEntityBase
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<IBasicAuthorizationHandler<TEntity, TRequest, TResponse>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }

        /// <summary>
        /// Adds an <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
        /// which will automatically be called by any <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddCrudAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TEntity : ConcurrentEntityBase
            where TGetListDto : ConcurrentDtoBase
            where TGetFullDto : ConcurrentDtoBase
            where TUpdateDto : ConcurrentDtoBase
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }

        /// <summary>
        /// Adds an <see cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}"/>
        /// which will automatically be called by any <see cref="ReadServiceBase{TContext, TEntity, TGetListDto, TGetFullDto}"/>.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddReadAuthorizationHandler<TAuthorizationhandler, TEntity, TGetListDto, TGetFullDto>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }
    }
}