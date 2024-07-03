using Microsoft.Extensions.Hosting;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IHostApplicationBuilder"/> and authorization handlers.
/// </summary>
public static class HostApplicationBuilderAuthorizationHandlerExtensions
{
    /// <summary>
    /// Adds an <see cref="IBasicAuthorizationHandler{TResponse}"/> which you
    /// can use from your custom service method.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddBasicAuthorizationHandler<TAuthorizationHandler, TResponse>(this IHostApplicationBuilder builder)
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TResponse>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddBasicAuthorizationHandler<TAuthorizationHandler, TResponse>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
    /// can use when calling <see cref="ServiceBase.TryExecuteWithAuthorizationAsync{T1, TResponse, TAuthorizationHandler}(T1, System.Func{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Func{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResultWithoutDb{T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Collections.Generic.IEnumerable{TAuthorizationHandler}, System.Threading.CancellationToken)"/> from your service method.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddBasicAuthorizationHandler<TAuthorizationHandler, TRequest, TResponse>(this IHostApplicationBuilder builder)
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddBasicAuthorizationHandler<TAuthorizationHandler, TRequest, TResponse>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
    /// can use when calling <see cref="DbServiceBase{TContext}.TryExecuteWithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse}, TAuthorizationHandler, System.Threading.CancellationToken, System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}}, System.Collections.Generic.IEnumerable{TAuthorizationHandler}, System.Threading.CancellationToken)"/> from your service method.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddBasicAuthorizationHandler<TAuthorizationHandler, TEntity, TRequest, TResponse>(this IHostApplicationBuilder builder)
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
        where TEntity : ConcurrentEntityBase
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddBasicAuthorizationHandler<TAuthorizationHandler, TEntity, TRequest, TResponse>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds an <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
    /// which will automatically be called by any <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCrudAuthorizationHandler<TAuthorizationHandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IHostApplicationBuilder builder)
        where TAuthorizationHandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddCrudAuthorizationHandler<TAuthorizationHandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}"/>
    /// which will automatically be called by any <see cref="ReadServiceBase{TContext, TEntity, TGetListDto, TGetFullDto}"/>.
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadAuthorizationHandler<TAuthorizationHandler, TEntity, TGetListDto, TGetFullDto>(this IHostApplicationBuilder builder)
        where TAuthorizationHandler : class, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddReadAuthorizationHandler<TAuthorizationHandler, TEntity, TGetListDto, TGetFullDto>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }
}