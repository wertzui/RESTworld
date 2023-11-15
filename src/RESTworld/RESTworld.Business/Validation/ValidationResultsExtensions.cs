using RESTworld.Business.Validation.Abstractions;
using System.Collections.Generic;

namespace RESTworld.Business.Validation;

/// <summary>
/// Contains extension methods for <see cref="IValidationResults"/>
/// </summary>
public static class ValidationResultsExtensions
{
    /// <summary>
    /// Adds a new failure to the existing <see cref="IValidationResults"/> if the validation failed and returns the given instance.
    /// </summary>
    /// <param name="validationResults">The existing <see cref="IValidationResults"/> to add the failure to if the validation failed.</param>
    /// <param name="validationSucceeded">Whether the validation succeeded or not.</param>
    /// <param name="path">The path being validated.</param>
    /// <param name="message">The message in case of a validation failure.</param>
    public static IValidationResults Validate(this IValidationResults validationResults, bool validationSucceeded, string path, string message)
    {
        if (!validationSucceeded)
            validationResults.AddValidationFailure(path, message);

        return validationResults;
    }
    /// <summary>
    /// Adds a new failure to the existing <see cref="IValidationResults"/> if the validation failed and returns the given instance.
    /// </summary>
    /// <param name="validationResults">The existing <see cref="IValidationResults"/> to add the failure to if the validation failed.</param>
    /// <param name="validationSucceeded">Whether the validation succeeded or not.</param>
    /// <param name="paths">The paths being validated.</param>
    /// <param name="message">The message in case of a validation failure.</param>
    public static IValidationResults Validate(this IValidationResults validationResults, bool validationSucceeded, IEnumerable<string> paths, string message)
    {
        if (!validationSucceeded)
        {
            foreach (var path in paths)
                validationResults.AddValidationFailure(path, message);
        }

        return validationResults;
    }
}
