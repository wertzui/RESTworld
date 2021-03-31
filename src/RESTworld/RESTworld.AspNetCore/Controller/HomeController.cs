using HAL.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace guenstiger.Table.Controller
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly IResourceFactory _resourceFactory;

        public HomeController(IResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Index() => Ok(_resourceFactory.CreateForHomeEndpointWithSwaggerUi("gt"));
    }
}