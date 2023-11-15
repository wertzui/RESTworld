using RESTworld.Business.Validation.Abstractions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace RESTworld.Business.Validation;

/// <inheritdoc/>
public class ValidationResults : IValidationResults
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _failures = new();

    /// <summary>
    /// Creates a new instance of the <see cref="ValidationResults"/> class.
    /// </summary>
    public ValidationResults()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ValidationResults"/> class and adds the given failure.
    /// </summary>
    public ValidationResults(string path, string message)
    {
        AddValidationFailure(path, message);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ValidationResults"/> class and adds the given exception.
    /// </summary>
    public ValidationResults(string path, Exception exception)
    {
        AddValidationFailure(path, exception);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ValidationResults"/> class and adds the given results.
    /// </summary>
    public ValidationResults(IValidationResults validationResults)
    {
        AddValidationFailures(validationResults);
    }

    /// <inheritdoc/>
    public int Count => _failures.Count;

    /// <inheritdoc/>
    public IEnumerable<string> Keys => _failures.Keys;

    /// <inheritdoc/>
    public bool ValidationSucceeded => Count == 0;

    /// <inheritdoc/>
    public IEnumerable<IReadOnlySet<string>> Values => _failures.Values;

    /// <inheritdoc/>
    public IReadOnlySet<string> this[string key] => _failures[key];

    /// <summary>
    /// Returns either a successful validation result or a failed result with the given path an
    /// message based on whether the validation succeeded or not.
    /// </summary>
    /// <param name="validationSucceeded">Whether the validation succeeded or not.</param>
    /// <param name="path">The path being validated. Use an empty string to add a global failure.</param>
    /// <param name="message">The message in case of a validation failure.</param>
    public static IValidationResults Validate(bool validationSucceeded, string path, string message)
        => new ValidationResults().Validate(validationSucceeded, path, message);

    /// <summary>
    /// Returns either a successful validation result or a failed result with the given path an
    /// message based on whether the validation succeeded or not.
    /// </summary>
    /// <param name="validationSucceeded">Whether the validation succeeded or not.</param>
    /// <param name="paths">The paths being validated.</param>
    /// <param name="message">The message in case of a validation failure.</param>
    public static IValidationResults Validate(bool validationSucceeded, IEnumerable<string> paths, string message)
        => new ValidationResults().Validate(validationSucceeded, paths, message);

    /// <summary>
    /// Returns either a successful validation result or a failed result with the given path an
    /// message based on whether the validation succeeded or not.
    /// </summary>
    /// <param name="validationSucceeded">Whether the validation succeeded or not.</param>
    /// <param name="path">The path being validated.</param>
    /// <param name="message">The message in case of a validation failure.</param>
    public static async Task<IValidationResults> ValidateAsync(Task<bool> validationSucceeded, string path, string message)
    {
        var result = await validationSucceeded;

        return Validate(result, path, message);
    }

    /// <inheritdoc/>
    public void AddCollectionValidationFailures(IEnumerable<IValidationResults> validationResults)
    {
        var i = 0;
        foreach (var result in validationResults)
        {
            foreach (var pair in result)
            {
                foreach (var message in pair.Value)
                {
                    var path = string.Concat("[", i, "].", pair.Key);
                    AddValidationFailure(path, message);
                }
            }

            i++;
        }
    }

    /// <inheritdoc/>
    public void AddValidationFailure(string path, string message)
    {
        var failuresForPath = _failures.GetOrAdd(path, _ => new HashSet<string>());
        failuresForPath.Add(message);
    }

    /// <inheritdoc/>
    public void AddValidationFailure(string path, Exception exception) => AddValidationFailure(path, exception.Message);

    /// <inheritdoc/>
    public void AddValidationFailures(IValidationResults validationResults)
    {
        foreach (var pair in validationResults)
        {
            foreach (var message in pair.Value)
            {
                AddValidationFailure(pair.Key, message);
            }
        }
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key) => _failures.ContainsKey(key);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, IReadOnlySet<string>>> GetEnumerator() => _failures.Select(p => new KeyValuePair<string, IReadOnlySet<string>>(p.Key, p.Value)).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IReadOnlySet<string> value)
    {
        var result = _failures.TryGetValue(key, out var set);
        value = set;

        return result;
    }
}