using Microsoft.EntityFrameworkCore;
using RESTworld.Business.Models;
using RESTworld.Business.Validation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;

namespace RESTworld.Business.Mapping;

/// <summary>
/// Translates a DbUpdateConcurrencyException into a ServiceResponse with a 409 Conflict status code and appropriate validation errors.
/// </summary>
/// <typeparam name="TEntity">The type of the entity for which exceptions are being translated.</typeparam>
/// <typeparam name="TGetFullDto">The type of the data transfer object (DTO) that represents the full details of the entity in service responses.</typeparam>
public class DbUpdateConcurrencyExceptionTranslator<TEntity, TGetFullDto> : ExceptionTranslator<TEntity, TGetFullDto>
{
    private readonly IMappingMemberNameProvider<TEntity, TGetFullDto> _memberNameProvider;

    /// <summary>
    /// Initializes a new instance of the DbUpdateConcurrencyExceptionTranslator class with the specified member name
    /// provider.
    /// </summary>
    /// <param name="memberNameProvider">An implementation of IMappingMemberNameProvider used to resolve mapping member names for the entity and DTO
    /// types.</param>
    /// <exception cref="ArgumentNullException">Thrown if memberNameProvider is null.</exception>
    public DbUpdateConcurrencyExceptionTranslator(IMappingMemberNameProvider<TEntity, TGetFullDto> memberNameProvider)
    {
        _memberNameProvider = memberNameProvider ?? throw new ArgumentNullException(nameof(memberNameProvider));
    }

    /// <inheritdoc/>
    public override bool TryTranslate(Exception exception, [NotNullWhen(true)] out ServiceResponse<TGetFullDto>? response)
    {
        // We only handle DbUpdateConcurrencyException
        if (exception is not DbUpdateConcurrencyException concurrencyException)
        {
            response = null;
            return false;
        }

        // First handle cases where no mapping is required.
        var validationResults = new ValidationResults("", "Concurrency validation failed. Please reload the resource.");
        response = ServiceResponse.FromFailedValidation<TGetFullDto>(HttpStatusCode.Conflict, validationResults);

        var entry = concurrencyException.Entries.FirstOrDefault();
        if (entry is null)
            return true;

        var concurrencyPropertyNames = entry.CurrentValues.Properties.Where(p => p.IsConcurrencyToken).Select(p => p.Name).ToHashSet();
        if (concurrencyPropertyNames.Count == 0)
            return true;

        var destinationMemberNames = _memberNameProvider.MemberMappingNames
            .Where(p => concurrencyPropertyNames.Contains(p.Key))
            .Select(p => p.Value)
            .ToList();

        if (destinationMemberNames.Count == 0)
            return true;

        foreach (var destinationMemberName in destinationMemberNames)
            validationResults.AddValidationFailure(destinationMemberName, "Concurrency validation failed.");

        return true;
    }
}
