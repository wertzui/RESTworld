using System;
using System.Collections.Generic;

namespace RESTworld.Business.Validation.Abstractions
{
    /// <summary>
    /// Contains the results of a validation.
    /// The validation is considered successful if no failures are present.
    /// </summary>
    public interface IValidationResults : IReadOnlyDictionary<string, IReadOnlySet<string>>
    {
        /// <summary>
        /// Adds a validation failure.
        /// </summary>
        /// <param name="path">The path to the validated property. Use an empty string to add a global failure.</param>
        /// <param name="exception">The exception. It's message will be added.</param>
        void AddValidationFailure(string path, Exception exception);

        /// <summary>
        /// Adds a validation failure.
        /// </summary>
        /// <param name="path">The path to the validated property. Use an empty string to add a global failure.</param>
        /// <param name="message">The message describing the validation failure.</param>
        void AddValidationFailure(string path, string message);

        /// <summary>
        /// Adds the given validation results to this instance.
        /// </summary>
        /// <param name="validationResults">The validation results to add to this instance.</param>
        void AddValidationFailures(IValidationResults validationResults);

        /// <summary>
        /// Adds the given validation results to this instance.
        /// All paths are prefixed with their index in square brackets and a dot.
        /// </summary>
        /// <param name="validationResults">The validation results to add to this instance.</param>
        void AddCollectionValidationFailures(IEnumerable<IValidationResults> validationResults);

        /// <summary>
        /// Whether the validation succeeded without any failures.
        /// </summary>
        bool ValidationSucceeded { get; }
    }
}