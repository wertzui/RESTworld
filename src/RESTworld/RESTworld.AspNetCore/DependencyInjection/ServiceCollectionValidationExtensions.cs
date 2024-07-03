using Microsoft.AspNetCore.Mvc.Infrastructure;
using RESTworld.AspNetCore.Results.Errors;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using RESTworld.AspNetCore.Validation;
using RESTworld.AspNetCore.Validation.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Validation;
using RESTworld.Business.Validation.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/> and validation.
/// </summary>
public static class ServiceCollectionValidationExtensions
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
    /// <param name="services">The <see cref="IServiceCollection"/> to add this validator to.</param>
    /// <returns>The <see cref="IServiceCollection"/> passed in.</returns>
    [Obsolete("Use HostApplicationBuilderValidationExtensions.AddCreateValidator instead.")]
    public static IServiceCollection AddCreateValidator<TValidator, TCreateDto, TEntity>(this IServiceCollection services)
        where TValidator : class, ICreateValidator<TCreateDto, TEntity>
        => services.AddScoped<ICreateValidator<TCreateDto, TEntity>, TValidator>();

    /// <summary>
    /// Adds the <see cref="RestWorldProblemDetailsFactory"/> as
    /// <see cref="IRestWorldProblemDetailsFactory"/> to the service collection and overrides
    /// the default <see cref="ProblemDetailsFactory"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [Obsolete("Use HostApplicationBuilderValidationExtensions.AddRestWorldProblemDetailsFactory instead.")]
    public static IServiceCollection AddRestWorldProblemDetailsFactory(this IServiceCollection services)
    {
        services.AddSingleton<ProblemDetailsFactory, RestWorldProblemDetailsFactory>();
        services.AddSingleton<IRestWorldProblemDetailsFactory, RestWorldProblemDetailsFactory>();

        return services;
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
    /// <param name="services">The <see cref="IServiceCollection"/> to add this validator to.</param>
    /// <returns>The <see cref="IServiceCollection"/> passed in.</returns>
    [Obsolete("Use HostApplicationBuilderValidationExtensions.AddUpdateValidator instead.")]
    public static IServiceCollection AddUpdateValidator<TValidator, TUpdateDto, TEntity>(this IServiceCollection services)
        where TValidator : class, IUpdateValidator<TUpdateDto, TEntity>
        => services.AddScoped<IUpdateValidator<TUpdateDto, TEntity>, TValidator>();

    /// <summary>
    /// Adds everything required to run validation and error handling. This includes the
    /// <see cref="RestWorldProblemDetailsFactory"/>, <see cref="IErrorResultFactory"/> and <see cref="IValidationService{TCreateDto, TUpdateDto, TEntity}"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [Obsolete("Use HostApplicationBuilderValidationExtensions.AddValidationAndErrorHandling instead.")]
    public static IServiceCollection AddValidationAndErrorHandling(this IServiceCollection services)
    {
        services.AddRestWorldProblemDetailsFactory();
        services.AddTransient<IErrorResultFactory, ErrorResultFactory>();

        // Add the validation service as open generic so it can automatically be picked up by
        // the CRUD service.
        services.AddScoped(typeof(IValidationService<,,>), typeof(ValidationService<,,>));

        return services;
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
    /// <param name="services">The <see cref="IServiceCollection"/> to add this validator to.</param>
    /// <returns>The <see cref="IServiceCollection"/> passed in.</returns>
    [Obsolete("Use HostApplicationBuilderValidationExtensions.AddValidator instead.")]
    public static IServiceCollection AddValidator<TValidator, TCreateDto, TUpdateDto, TEntity>(this IServiceCollection services)
        where TValidator : class, IValidator<TCreateDto, TUpdateDto, TEntity>
    {
        services.AddCreateValidator<TValidator, TCreateDto, TEntity>();
        services.AddUpdateValidator<TValidator, TUpdateDto, TEntity>();
        services.AddScoped<IValidator<TCreateDto, TUpdateDto, TEntity>, TValidator>();

        return services;
    }
}