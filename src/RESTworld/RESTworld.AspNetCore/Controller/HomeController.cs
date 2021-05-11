using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace guenstiger.Table.Controller
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly IResourceFactory _resourceFactory;
        private readonly ILinkFactory _linkFactory;

        public HomeController(IResourceFactory resourceFactory, ILinkFactory linkFactory)
        {
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual IActionResult Index()
        {
            var resource = _resourceFactory.CreateForHomeEndpointWithSwaggerUi("gt");

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