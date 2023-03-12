using RESTworld.Business.Validation.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace RESTworld.Business.Validation
{
    /// <summary>
    /// A successful validation that cannot be altered.
    /// </summary>
    public class SuccessfullValidationResults : IValidationResults
    {
        private static readonly IReadOnlySet<string> _emptySet = ImmutableHashSet.Create<string>();

        private SuccessfullValidationResults()
        {
        }

        /// <summary>
        /// The single one instance.
        /// </summary>
        public static IValidationResults Instance { get; } = new SuccessfullValidationResults();

        /// <inheritdoc/>
        public int Count => 0;

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _emptySet;

        /// <inheritdoc/>
        public bool ValidationSucceeded => true;

        /// <inheritdoc/>
        public IEnumerable<IReadOnlySet<string>> Values => Array.Empty<IReadOnlySet<string>>();

        /// <inheritdoc/>
        public IReadOnlySet<string> this[string key] => _emptySet;

        /// <inheritdoc/>
        public void AddCollectionValidationFailures(IEnumerable<IValidationResults> validationResults) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void AddValidationFailure(string path, Exception exception) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void AddValidationFailure(string path, string message) => throw new NotSupportedException();

        /// <inheritdoc/>
        public void AddValidationFailures(IValidationResults validationResults) => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool ContainsKey(string key) => false;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, IReadOnlySet<string>>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, IReadOnlySet<string>>>)Array.Empty<KeyValuePair<string, IReadOnlySet<string>>>()).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out IReadOnlySet<string> value)
        {
            value = null;
            return false;
        }
    }
}