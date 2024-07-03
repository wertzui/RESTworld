using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using RESTworld.AspNetCore.Validation;
using RESTworld.AspNetCore.Validation.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Validation.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IHostApplicationBuilder"/> and validation.
/// </summary>
public static class HostApplicationBuilderValidationExtensions
{
    /// <summary>
    /// Adds an <see cref="ICreateValidator{TCreateDto, TEntity}"/> to the service collection.
    /// It is automatically used in any
    /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
    /// for validation.
    /// </summary>
    /// <typeparam name="TValidator">The type of the validator.</typeparam>
    /// <typeparam name="TCreateDto">The type of the Create DTO.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> passed in.</returns>
    public static IHostApplicationBuilder AddCreateValidator<TValidator, TCreateDto, TEntity>(this IHostApplicationBuilder builder)
        where TValidator : class, ICreateValidator<TCreateDto, TEntity>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddCreateValidator<TValidator, TCreateDto, TEntity>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds the <see cref="RestWorldProblemDetailsFactory"/> as
    /// <see cref="IRestWorldProblemDetailsFactory"/> to the service collection and overrides
    /// the default <see cref="ProblemDetailsFactory"/>.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> passed in.</returns>
    public static IHostApplicationBuilder AddRestWorldProblemDetailsFactory(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddRestWorldProblemDetailsFactory();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IUpdateValidator{TUpdateDto, TEntity}"/> to the service collection.
    /// It is automatically used in any
    /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
    /// for validation.
    /// </summary>
    /// <typeparam name="TValidator">The type of the validator.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the Update DTO.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> passed in.</returns>
    public static IHostApplicationBuilder AddUpdateValidator<TValidator, TUpdateDto, TEntity>(this IHostApplicationBuilder builder)
        where TValidator : class, IUpdateValidator<TUpdateDto, TEntity>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddUpdateValidator<TValidator, TUpdateDto, TEntity>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds everything required to run validation and error handling. This includes the
    /// <see cref="RestWorldProblemDetailsFactory"/>, <see cref="IErrorResultFactory"/> and <see cref="IValidationService{TCreateDto, TUpdateDto, TEntity}"/>.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> passed in.</returns>
    public static IHostApplicationBuilder AddValidationAndErrorHandling(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddValidationAndErrorHandling();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> to the service
    /// collection. It is automatically used in any
    /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
    /// for validation.
    /// </summary>
    /// <typeparam name="TValidator">The type of the validator.</typeparam>
    /// <typeparam name="TCreateDto">The type of the Create DTO.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the Update DTO.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> passed in.</returns>
    public static IHostApplicationBuilder AddValidator<TValidator, TCreateDto, TUpdateDto, TEntity>(this IHostApplicationBuilder builder)
        where TValidator : class, IValidator<TCreateDto, TUpdateDto, TEntity>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddValidator<TValidator, TCreateDto, TUpdateDto, TEntity>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }
}