using System.Collections.Generic;

namespace RESTworld.Business.Mapping;

/// <summary>
/// Defines a provider that supplies a mapping between source member names and their corresponding mapped names for use
/// in data transfer or transformation scenarios.
/// </summary>
/// <typeparam name="TSource">The type of the source entity whose member names are being mapped. Normally this is the Entity.</typeparam>
/// <typeparam name="TTarget">The type of the target entity to which the source member names are mapped. Normally this the the GetFullDto.</typeparam>
public interface IMappingMemberNameProvider<TSource, TTarget>
{
    /// <summary>
    /// Gets a read-only dictionary that maps member names to their corresponding mapped names.
    /// </summary>
    /// <remarks>
    /// This dictionary provides the mapping between entity member names and their mapped representations in the GetFullDto.
    /// It is used when translating database errors to user-friendly messages, allowing the system to identify which member caused the error and provide a more informative response.
    /// </remarks>
    public IReadOnlyDictionary<string, string> MemberMappingNames { get; }
}
