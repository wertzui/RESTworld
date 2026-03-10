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
/// Contains extension methods for <see cref="IHostApplicationBuilder"/> and CRUD controllers and services.
/// </summary>
public static class HostApplicationBuilderCrudPipelineExtensions
{
    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="CrudController{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and
    /// the <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a queries on a List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCrudPipeline<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>(this IHostApplicationBuilder builder, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddCrudPipeline<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>(apiVersion, isDeprecated);

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="CrudController{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>, the
    /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and the <typeparamref name="TAuthorizationHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a queries on a List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="apiVersion">An optional API version.</param>
    /// <param name="isDeprecated">
    /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TAuthorizationHandler>(this IHostApplicationBuilder builder, ApiVersion? apiVersion = null, bool isDeprecated = false)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TAuthorizationHandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddCrudPipelineWithAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TAuthorizationHandler>(builder.Configuration, apiVersion, isDeprecated);

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline without authorization, using the <see cref="CrudController{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and a
    /// custom <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a queries on a List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> implementation.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TService>(this IHostApplicationBuilder builder)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TService : class, ICrudServiceBase<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddCrudPipelineWithCustomService<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();

        return builder;
    }

    /// <summary>
    /// Adds a complete CRUD pipeline with authorization, using the <see cref="CrudController{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>, a
    /// custom <typeparamref name="TService"/> and the <typeparamref name="TAuthorizationHandler"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for a queries on a List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCrudPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TService, TAuthorizationHandler>(this IHostApplicationBuilder builder)
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TService : class, ICrudServiceBase<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TAuthorizationHandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddCrudPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TService, TAuthorizationHandler>(builder.Configuration);

        return builder;
    }
}