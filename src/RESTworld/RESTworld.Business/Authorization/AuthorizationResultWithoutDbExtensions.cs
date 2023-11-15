using System.Net;

namespace RESTworld.Business.Authorization;

/// <summary>
/// Contains extension methods for <see cref="AuthorizationResultWithoutDb"/> and the different generic overloads.
/// </summary>
public static class AuthorizationResultWithoutDbExtensions
{
    /// <summary>
    /// Returns a new authorization result with the same value, but the new status.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="status">The status.</param>
    /// <returns></returns>
    public static AuthorizationResultWithoutDb<T1> WithStatus<T1>(this AuthorizationResultWithoutDb<T1> previousResult, HttpStatusCode status)
        => new(status, previousResult.Value1);

    /// <summary>
    /// Returns a new authorization result with the same values, but the new status.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="status">The status.</param>
    /// <returns></returns>
    public static AuthorizationResultWithoutDb<T1, T2> WithStatus<T1, T2>(this AuthorizationResultWithoutDb<T1, T2> previousResult, HttpStatusCode status)
        => new(status, previousResult.Value1, previousResult.Value2);
}