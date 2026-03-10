using RESTworld.Business.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTworld.Business.Mapping;

/// <summary>
/// An IExceptionTranslator is responsible for translating exceptions into ServiceResponses that can be returned to the client.
/// This allows for a centralized and consistent way to handle exceptions across the application, and to provide meaningful error messages and status codes to the client.
/// </summary>
public interface IExceptionTranslator
{
    /// <summary>
    /// Attempts to translate the specified exception into a standardized service response.
    /// </summary>
    /// <param name="exception">The exception to translate. Cannot be null.</param>
    /// <param name="response">When this method returns <see langword="true"/>, contains the translated <see cref="ServiceResponse{Object}"/>;
    /// otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the exception was successfully translated; otherwise, <see langword="false"/>.</returns>
    public bool TryTranslate(Exception exception, [NotNullWhen(true)] out ServiceResponse<object>? response);
}

/// <summary>
/// Defines a mechanism for translating exceptions into service responses for a specific entity and its corresponding
/// data transfer object (DTO).
/// </summary>
/// <typeparam name="TEntity">The type of the entity for which exceptions are being translated.</typeparam>
/// <typeparam name="TGetFullDto">The type of the data transfer object (DTO) that represents the full details of the entity in service responses.</typeparam>
public interface IExceptionTranslator<TEntity, TGetFullDto> : IExceptionTranslator
{
    /// <summary>
    /// Attempts to translate the specified exception into a standardized service response specific to the entity and its corresponding DTO.
    /// </summary>
    /// <param name="exception">The exception to translate. Cannot be null.</param>
    /// <param name="response">When this method returns <see langword="true"/>, contains the translated <see cref="ServiceResponse{TGetFullDto}"/>;
    /// otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the exception was successfully translated; otherwise, <see langword="false"/>.</returns>
    public bool TryTranslate(Exception exception, [NotNullWhen(true)] out ServiceResponse<TGetFullDto>? response);
}
