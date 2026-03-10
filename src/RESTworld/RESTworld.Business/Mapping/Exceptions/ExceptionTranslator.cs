using RESTworld.Business.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTworld.Business.Mapping;

/// <summary>
/// Base class for exception translators that provides a default implementation for the non-generic TryTranslate method, which delegates to the generic TryTranslate method.
/// This allows derived classes to only implement the generic TryTranslate method, while still being compatible with the IExceptionTranslator interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity for which exceptions are being translated.</typeparam>
/// <typeparam name="TGetFullDto">The type of the data transfer object (DTO) that represents the full details of the entity in service responses.</typeparam>
public abstract class ExceptionTranslator<TEntity, TGetFullDto> : IExceptionTranslator<TEntity, TGetFullDto>
{
    /// <inheritdoc/>
    public abstract bool TryTranslate(Exception exception, [NotNullWhen(true)] out ServiceResponse<TGetFullDto>? response);

    /// <inheritdoc/>
    public bool TryTranslate(Exception exception, [NotNullWhen(true)] out ServiceResponse<object>? response)
    {
        if (TryTranslate(exception, out ServiceResponse<TGetFullDto>? typedResponse))
        {
            response = typedResponse.ChangeType<object>();
            return true;
        }

        response = null;
        return false;
    }
}
