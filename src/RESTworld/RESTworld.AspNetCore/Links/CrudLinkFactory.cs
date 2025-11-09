using HAL.AspNetCore;
using HAL.AspNetCore.Utils;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.Links.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Linq;
using System.Net.Http;

namespace RESTworld.AspNetCore.Links
{

    /// <inheritdoc/>
    public class CrudLinkFactory : LinkFactory, ICrudLinkFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrudLinkFactory"/> class.
        /// </summary>
        /// <param name="linkGenerator">The link generator from ASP.Net Core.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="apiExplorer">The API explorer.</param>
        public CrudLinkFactory(
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor,
            IApiDescriptionGroupCollectionProvider apiExplorer)
            : base(linkGenerator, httpContextAccessor, apiExplorer)
        {
        }

        /// <inheritdoc/>
        public Resource<TDto> AddDeleteLink<TDto>(Resource<TDto> resource)
            where TDto : DtoBase?
        {
            ArgumentNullException.ThrowIfNull(resource);

            if (resource.State is not ConcurrentDtoBase dto || dto.Timestamp is null)
                return resource;

            var id = dto.Id;
            var timestamp = Base64UrlTextEncoder.Encode(dto.Timestamp);

            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext is null)
                return resource;

            var href = LinkGenerator.GetUriByAction(httpContext, HttpMethod.Delete.Method, values: new { id, timestamp });

            if (href is null)
                return resource;

            return resource.AddLink("delete", new Link(href) { Name = HttpMethod.Delete.Method });
        }

        /// <inheritdoc/>
        public TFormsResource AddDeleteLink<TFormsResource>(TFormsResource resource)
            where TFormsResource : FormsResource
        {
            ArgumentNullException.ThrowIfNull(resource);

            if (!resource.Templates.TryGetValue("default", out var template))
                return resource;

            var id = template.Properties?.FirstOrDefault(p => p.Name == nameof(DtoBase.Id))?.Value;
            if (id is null)
                return resource;

            if (template.Properties?.FirstOrDefault(p => p.Name == nameof(ConcurrentDtoBase.Timestamp))?.Value is not byte[] timestampBytes)
                return resource;

            var timestamp = Base64UrlTextEncoder.Encode(timestampBytes);

            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext is null)
                return resource;

            var href = LinkGenerator.GetUriByAction(httpContext, HttpMethod.Delete.Method, values: new { id, timestamp });
            if (href is null)
                return resource;

            return resource.AddLink("delete", new Link(href) { Name = HttpMethod.Delete.Method });
        }

        /// <inheritdoc/>
        public TResource AddNewLink<TResource>(TResource resource)
            where TResource : Resource
        {
            ArgumentNullException.ThrowIfNull(resource);

            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext is null)
                return resource;

            var href = LinkGenerator.GetUriByAction(httpContext, "new");
            if (href is null)
                return resource;

            return resource.AddLink(new Link(href) { Name = "new" });
        }

        /// <inheritdoc/>
        public Resource<TDto> AddSaveAndDeleteLinks<TDto>(Resource<TDto> resource)
            where TDto : DtoBase?
        {
            ArgumentNullException.ThrowIfNull(resource);

            AddSaveLink(resource);
            AddDeleteLink(resource);

            return resource;
        }

        /// <inheritdoc/>
        public Resource<TDto> AddSaveLink<TDto>(Resource<TDto> resource)
            where TDto : DtoBase?
        {
            ArgumentNullException.ThrowIfNull(resource);

            if (resource.State is null)
                return resource;

            var id = resource.State.Id;

            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext is null)
                return resource;

            var href = LinkGenerator.GetUriByAction(httpContext, HttpMethod.Put.Method, values: new { id });
            if (href is null)
                return resource;

            return resource.AddLink(
                "save",
                new Link(href)
                {
                    Name = HttpMethod.Put.Method,
                });
        }

        /// <inheritdoc/>
        public TResource AddHistoryLink<TResource, TDto>(TResource resource, TDto dto)
            where TResource : Resource
            where TDto : DtoBase?
        {
            ArgumentNullException.ThrowIfNull(resource);

            if (dto is null)
                return resource;

            var id = dto.Id;

            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext is null)
                return resource;

            var href = LinkGenerator.GetUriByAction(httpContext, ActionHelper.StripAsyncSuffix(nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetHistoryAsync)), values: new { filter = $"id eq {id}" });
            if (href is null)
                return resource;

            return resource.AddLink(
                "history",
                new Link(href)
                {
                    Name = "history"
                });
        }

        /// <inheritdoc/>
        public Resource<TDto> AddHistoryLink<TDto>(Resource<TDto> resource)
            where TDto : DtoBase?
            => AddHistoryLink(resource, resource.State);
    }
}