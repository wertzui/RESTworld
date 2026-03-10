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
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="ReadController{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/> and
    /// the <see cref="ReadServiceBase{TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a query.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipeline<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto>(this IHostApplicationBuilder builder, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddReadPipeline<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto>(apiVersion, isDeprecated);

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="ReadController{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>, the
    /// <see cref="ReadServiceBase{TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto}"/> and the <typeparamref name="TAuthorizationHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a query.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="IReadAuthorizationHandler{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipelineWithAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TAuthorizationHandler>(this IHostApplicationBuilder builder, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TAuthorizationHandler : class, IReadAuthorizationHandler<TEntity, TQueryDto, TGetListDto, TGetFullDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddReadPipelineWithAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TAuthorizationHandler>(builder.Configuration, apiVersion, isDeprecated);

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="ReadController{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/> and a
    /// custom <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a query.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="IReadServiceBase{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/> implementation.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipelineWithCustomService<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService>(this IHostApplicationBuilder builder)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TService : class, IReadServiceBase<TEntity, TQueryDto, TGetListDto, TGetFullDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddReadPipelineWithCustomService<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService>();

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="ReadController{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>, a
    /// custom <typeparamref name="TService"/> and the <typeparamref name="TAuthorizationHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a query.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="IReadServiceBase{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="IReadAuthorizationHandler{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddReadPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService, TAuthorizationHandler>(this IHostApplicationBuilder builder)
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase

        where TService : class, IReadServiceBase<TEntity, TQueryDto, TGetListDto, TGetFullDto>
        where TAuthorizationHandler : class, IReadAuthorizationHandler<TEntity, TQueryDto, TGetListDto, TGetFullDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddReadPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto, TService, TAuthorizationHandler>(builder.Configuration);

        return builder;
    }
}