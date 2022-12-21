using Asp.Versioning;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Controllers;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// The HomeController is the one that is used for the root of you application.
    /// It will output links to all endpoints of all controllers.
    /// </summary>
    [Route("")]
    [ApiVersionNeutral]
    public class HomeController : HalControllerBase
    {
        private readonly string _curieName;
        private readonly ILinkFactory _linkFactory;
        private readonly IResourceFactory _resourceFactory;
        private static HomeDto? _state;

        /// <summary>
        /// Creates a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="resourceFactory">The factory used to create resources.</param>
        /// <param name="linkFactory">The factory used to create links.</param>
        /// <param name="options">Options which contain the curie.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        public HomeController(
            IResourceFactory resourceFactory,
            ILinkFactory linkFactory,
            IOptions<RestWorldOptions> options,
            IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (apiExplorer is null)
                throw new ArgumentNullException(nameof(apiExplorer));

            _curieName = options.Value.GetCurieOrDefault();
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));

            if (_state is null)
                _state = CreateState(apiExplorer);
        }

        /// <summary>
        /// Returns a list of links to all controller endpoints.
        /// </summary>
        /// <returns>A list of links to all controller endpoints.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
        public virtual IActionResult Index(ApiVersion version)
        {
            var resource = _resourceFactory.CreateForHomeEndpointWithSwaggerUi(_state, _curieName, version);

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

        private HomeDto CreateState(IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            var descriptorGroups = apiExplorer.ApiDescriptionGroups.Items;

            var versions = descriptorGroups
                .SelectMany(g => g.Items)
                .Select(i => i.ActionDescriptor.GetProperty<ApiVersionModel>())
                .Where(v => v is not null)
                .Cast<ApiVersionModel>()
                .Distinct()
                .ToList();

            var deprecatedVersions = versions
                .SelectMany(v => v.DeprecatedApiVersions)
                .Select(v => v.ToString("V"))
                .ToHashSet();

            var supportedVersions = versions
                .SelectMany(v => v.ImplementedApiVersions)
                .Select(v => v.ToString("V"))
                .ToHashSet();

            // If a version is deprecated, it is not supported.
            supportedVersions.ExceptWith(deprecatedVersions);

            var versionInformation = new VersionInformationDto(supportedVersions, deprecatedVersions);

            return new HomeDto(versionInformation);
        }
    }
}