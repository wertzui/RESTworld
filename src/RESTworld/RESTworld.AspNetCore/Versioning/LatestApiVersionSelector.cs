using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using System.Linq;

namespace RESTworld.AspNetCore.Versioning
{
    /// <summary>
    /// This selector will always select the latest API version that is implemented by the controller.
    /// It is used if the DefaultApiVersion in the appsettings is specified as "latest".
    /// </summary>
    /// <seealso cref="IApiVersionSelector" />
    public class LatestApiVersionSelector : IApiVersionSelector
    {
        /// <inheritdoc/>
        public ApiVersion SelectVersion(HttpRequest request, ApiVersionModel model) => model.ImplementedApiVersions.OrderByDescending(v => v).FirstOrDefault();
    }
}