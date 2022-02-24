using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RESTworld.Common.Dtos;
using System.Linq;
using System.Net.Http;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// Contains extension methods for <see cref="IUrlHelper"/>.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Adds a delete link to the given resource if the state of the resource is a <see cref="ConcurrentDtoBase"/>.
        /// </summary>
        /// <typeparam name="TDto">The type of the resource.</typeparam>
        /// <param name="url">The URL helper which is used to generate the link.</param>
        /// <param name="resource">The resource to add the link to.</param>
        public static void AddDeleteLink<TDto>(this IUrlHelper url, Resource<TDto> resource)
            where TDto : DtoBase?
        {
            if (resource.State is not ConcurrentDtoBase dto || dto.Timestamp is null)
                return;

            var id = dto.Id;
            var timestamp = Base64UrlTextEncoder.Encode(dto.Timestamp);


            var href = url.ActionLink(HttpMethod.Delete.Method, values: new { id, timestamp });
            if (href is null)
                return;

            resource.AddLink("delete", new Link(href) { Name = HttpMethod.Delete.Method });
        }

        /// <summary>
        /// Adds a delete link to the given resource.
        /// </summary>
        /// <param name="url">The URL helper which is used to generate the link.</param>
        /// <param name="resource">The resource to add the link to.</param>
        public static void AddDeleteLink(this IUrlHelper url, FormsResource resource)
        {
            if (!resource.Templates.TryGetValue("default", out var template))
                return;

            var id = template.Properties?.FirstOrDefault(p => p.Name == nameof(DtoBase.Id))?.Value;
            if (id is null)
                return;

            if (template.Properties?.FirstOrDefault(p => p.Name == nameof(ConcurrentDtoBase.Timestamp))?.Value is not byte[] timestampBytes)
                return;

            var timestamp = Base64UrlTextEncoder.Encode(timestampBytes);

            var href = url.ActionLink(HttpMethod.Delete.Method, values: new { id, timestamp });
            if (href is null)
                return;

            resource.AddLink("delete", new Link(href) { Name = HttpMethod.Delete.Method });
        }

        /// <summary>
        /// Adds the "new" link to the resource which is used to retrieve a template for creating new entries.
        /// </summary>
        /// <param name="url">The URL helper which is used to generate the link.</param>
        /// <param name="resource">The resource to add the link to.</param>
        public static void AddNewLink(this IUrlHelper url, Resource resource)
        {
            var href = url.ActionLink("new");
            if (href is not null)
                resource.AddLink(new Link(href) { Name = "new" });
        }

        /// <summary>
        /// Adds a save link to the given resource.
        /// Adds a delete link to the given resource if the state is a <see cref="ConcurrentDtoBase"/>.
        /// </summary>
        /// <typeparam name="TDto">The type of the resource.</typeparam>
        /// <param name="url">The URL helper which is used to generate the link.</param>
        /// <param name="resource">The resource to add the links to.</param>
        public static void AddSaveAndDeleteLinks<TDto>(this IUrlHelper url, Resource<TDto> resource)
            where TDto : DtoBase?
        {
            url.AddSaveLink(resource);
            url.AddDeleteLink(resource);
        }

        /// <summary>
        /// Adds a save link to the given resource.
        /// </summary>
        /// <typeparam name="TDto">The type of the resource.</typeparam>
        /// <param name="url">The URL helper which is used to generate the link.</param>
        /// <param name="resource">The resource to add the link to.</param>
        public static void AddSaveLink<TDto>(this IUrlHelper url, Resource<TDto> resource)
            where TDto : DtoBase?
        {
            if (resource.State is null)
                return;

            var id = resource.State.Id;

            var href = url.ActionLink(HttpMethod.Put.Method, values: new { id });
            if (href is null)
                return;

            resource.AddLink(
                "save",
                new Link(href)
                {
                    Name = HttpMethod.Put.Method,
                });
        }
    }
}
