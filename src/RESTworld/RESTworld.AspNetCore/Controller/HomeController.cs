using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Controllers;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace guenstiger.Table.Controller
{
    /// <summary>
    /// The HomeController is the one that is used for the root of you application.
    /// It will output links to all endpoints of all controllers.
    /// </summary>
    [Route("")]
    public class HomeController : HalControllerBase
    {
        private readonly string _curieName;
        private readonly IResourceFactory _resourceFactory;
        private readonly ILinkFactory _linkFactory;

        /// <summary>
        /// Creates a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="resourceFactory">The factory used to create resources.</param>
        /// <param name="linkFactory">The factory used to create links.</param>
        /// <param name="options">Options which contain the curie.</param>
        public HomeController(IResourceFactory resourceFactory, ILinkFactory linkFactory, IOptions<RestWorldOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            _curieName = GetCurieNameOrDefault(options.Value.Curie);

            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        private string GetCurieNameOrDefault(string curieName)
        {
            if (!string.IsNullOrWhiteSpace(curieName))
                return curieName;

            return string.Concat(Assembly.GetEntryAssembly().GetName().Name.Where(c => char.IsUpper(c))).ToLowerInvariant();
        }

        /// <summary>
        /// Returns a list of links to all controller endpoints.
        /// </summary>
        /// <returns>A list of links to all controller endpoints.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
        public virtual IActionResult Index()
        {
            var resource = _resourceFactory.CreateForHomeEndpointWithSwaggerUi(_curieName);

            var startup = _linkFactory.Create(name: "startup");
            startup.Href += "health/startup";
            resource.AddLink("health", startup);

            var live = _linkFactory.Create(name: "live");
            live.Href += "health/live";
            resource.AddLink("health", live);

            var ready = _linkFactory.Create(name: "ready");
            ready.Href += "health/ready";
            resource.AddLink("health", ready);

            return Ok(resource);
        }
    }
}