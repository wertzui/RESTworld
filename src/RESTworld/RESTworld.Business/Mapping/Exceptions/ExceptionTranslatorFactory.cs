using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RESTworld.Business.Mapping.Exceptions;

/// <summary>
/// Provides factory methods for creating collections of exception translators that handle database concurrency and
/// foreign key constraint exceptions for specific entity and context types.
/// </summary>
/// <remarks>
/// Use this class to obtain exception translators tailored to your entity and database context types,
/// enabling consistent handling of common database exceptions such as concurrency conflicts and foreign key violations.
/// The returned translators can be used to translate low-level database exceptions into application-specific exceptions
/// or error responses.
/// </remarks>
public static class ExceptionTranslatorFactory
{

    /// <summary>
    /// Creates a collection of exception translators for handling database concurrency and foreign key constraint
    /// exceptions related to the specified entity type.
    /// </summary>
    /// <typeparam name="TContext">The type of the database context used for exception translation.</typeparam>
    /// <typeparam name="TEntity">The type of the entity for which exceptions will be translated.</typeparam>
    /// <typeparam name="TQueryDto">The type of the data transfer object used for query operations.</typeparam>
    /// <typeparam name="TGetListDto">The type of the data transfer object used for listing entities.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the data transfer object used for detailed entity information.</typeparam>
    /// <param name="mapper">The mapper used to convert entities to their corresponding data transfer objects.</param>
    /// <param name="contextFactory">The factory used to create database context instances for exception translation.</param>
    /// <returns>A read-only collection of exception translators applicable to the specified entity and context types.</returns>
    public static IReadOnlyCollection<IExceptionTranslator> CreateExceptionTranslators<TContext, TEntity, TQueryDto, TGetListDto, TGetFullDto>(IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto> mapper, IDbContextFactory<TContext> contextFactory)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(contextFactory);

        return
        [
            new DbUpdateConcurrencyExceptionTranslator<TEntity, TGetFullDto>(mapper),
            new ForeignKeyConstraintExceptionTranslator<TContext, TEntity, TGetFullDto>(mapper, contextFactory)
        ];
    }
}
