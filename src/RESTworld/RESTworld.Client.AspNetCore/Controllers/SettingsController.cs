using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.Controller;
using RESTworld.Common.Client;

namespace RESTworld.Client.AspNetCore.Controllers
{
    /// <summary>
    /// This controller serves the <see cref="ClientSettings"/> which the Angular application will use to find the correct API endpoints.
    /// </summary>
    [Route("[controller]")]
    public class SettingsController : RestControllerBase
    {
        private readonly IOptions<RestWorldClientOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="options">RestWorldOptions from the app config.</param>
        /// <param name="resourceFactory">The resource factory.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public SettingsController(IOptions<RestWorldClientOptions> options, IODataResourceFactory resourceFactory)
            : base(resourceFactory)
        {
            _options = options ?? throw new System.ArgumentNullException(nameof(options));
        }
        /// <summary>
        /// Gets the <see cref="ClientSettings"/> which the Angular application will use to find the correct API endpoints.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        public ActionResult<Resource<ClientSettings?>> Get()
        {
            var resource = ResourceFactory.CreateForGetEndpoint(_options.Value.ClientSettings);

            return resource;
        }
    }
}
