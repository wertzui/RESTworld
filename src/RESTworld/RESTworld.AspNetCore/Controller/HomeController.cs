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
    [Route("")]
    public class HomeController : HalControllerBase
    {
        private readonly string _curieName;
        private readonly IResourceFactory _resourceFactory;
        private readonly ILinkFactory _linkFactory;

        public HomeController(IResourceFactory resourceFactory, ILinkFactory linkFactory, IOptions<RestWorldOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            _curieName = GetCurieNameOrDefault(options.Value.CurieName);

            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        private string GetCurieNameOrDefault(string curieName)
        {
            if (!string.IsNullOrWhiteSpace(curieName))
                return curieName;

            return string.Concat(Assembly.GetEntryAssembly().GetName().Name.Where(c => char.IsUpper(c))).ToLowerInvariant();
        }

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