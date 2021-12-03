namespace RESTworld.Common.Dtos
{
    /// <summary>
    /// This is displayed on the Home endpoint.
    /// </summary>
    /// <param name="Versions">Information about supported versions.</param>
    public record HomeDto(VersionInformationDto Versions);
}