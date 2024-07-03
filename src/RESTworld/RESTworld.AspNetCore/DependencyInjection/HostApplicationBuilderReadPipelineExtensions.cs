using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Controller;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/> and Read controllers and services.
/// </summary>
public static class HostApplicationBuilderReadPipelineExtensions
{
    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/> and
    /// the <see cref="ReadServiceBase{TContext, TEntity, TGetListDto, TGetFullDto}"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipeline<TContext, TEntity, TGetListDto, TGetFullDto>(this IHostApplicationBuilder builder, ApiVersion? apiVersion = null, bool isDeprecated = false)
                where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddReadPipeline<TContext, TEntity, TGetListDto, TGetFullDto>(apiVersion, isDeprecated);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/>, the
    /// <see cref="ReadServiceBase{TContext, TEntity, TGetListDto, TGetFullDto}"/> and the <typeparamref name="TAuthorizationHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipelineWithAuthorization<TContext, TEntity, TGetListDto, TGetFullDto, TAuthorizationHandler>(this IHostApplicationBuilder builder, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TAuthorizationHandler : class, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddReadPipelineWithAuthorization<TContext, TEntity, TGetListDto, TGetFullDto, TAuthorizationHandler>(builder.Configuration, apiVersion, isDeprecated);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
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
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipelineWithCustomService<TContext, TEntity, TGetListDto, TGetFullDto, TService>(this IHostApplicationBuilder builder)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TService : class, IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddReadPipelineWithCustomService<TContext, TEntity, TGetListDto, TGetFullDto, TService>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/>, a
    /// custom <typeparamref name="TService"/> and the <typeparamref name="TAuthorizationHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="IReadServiceBase{TEntity, TGetListDto, TGetFullDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TGetListDto, TGetFullDto, TService, TAuthorizationHandler>(this IHostApplicationBuilder builder)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TService : class, IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
        where TAuthorizationHandler : class, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddReadPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TGetListDto, TGetFullDto, TService, TAuthorizationHandler>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }
}