using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.DependencyInjection
{
    /// <summary>
    /// Options to configure the behavior of RESTWorld.
    /// </summary>
    public class RestWorldOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the total count should be calculated for list endpoints. Calculating the total count means a second roundtrip to the database when calling the list endpoint.
        /// </summary>
        public bool CalculateTotalCountForListEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the curie name which is used to generate the prefix (the part before the ':') for your links on the home controller.
        /// </summary>
        public string? Curie { get; set; }

        /// <summary>
        /// Disables all <see cref="Business.Authorization.Abstractions.ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s so they wont be called.
        /// You can use this in test setups where you want to disable the authorization for debugging purposes.
        /// You should never set this in a production environment!
        /// </summary>
        public bool DisableAuthorization { get; set; }

        /// <summary>
        /// Gets or sets the maximum number items to return at the list endpoint.
        /// </summary>
        public int MaxNumberForListEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the versioning options.
        /// </summary>
        /// <value>
        /// The versioning options.
        /// </value>
        public VersioningOptions? Versioning { get; set; }

        /// <summary>
        /// Gets the curie if set, or default which is the upper case letters of the entry assembly as lower case (MyEntryAssembly => "mea").
        /// </summary>
        /// <returns></returns>
        public string GetCurieOrDefault()
        {
            if (!string.IsNullOrWhiteSpace(Curie))
                return Curie;

            return string.Concat(Assembly.GetEntryAssembly()!.GetName().Name!.Where(c => char.IsUpper(c))).ToLowerInvariant();
        }
    }
}