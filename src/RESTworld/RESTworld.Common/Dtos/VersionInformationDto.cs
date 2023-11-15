using System.Collections.Generic;

namespace RESTworld.Common.Dtos;

/// <summary>
/// Used to display information about supported version on the Home Controller.
/// </summary>
/// <param name="Supported">The currently supported versions.</param>
/// <param name="Deprecated">These versions are deprecated and will be removed in future versions.</param>
public record VersionInformationDto(HashSet<string> Supported, HashSet<string> Deprecated)
{
}