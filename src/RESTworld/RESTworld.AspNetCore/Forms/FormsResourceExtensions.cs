using HAL.Common.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RESTworld.AspNetCore.Forms
{
    /// <summary>
    /// Contains extensions methods for <see cref="FormsResource"/>.
    /// </summary>
    public static class FormsResourceExtensions
    {
        /// <summary>
        /// Adds a reload template to the given <see cref="FormsResource"/>.
        /// </summary>
        /// <param name="resource">The resource to add the template to.</param>
        /// <param name="keyOfTemplateToAddDeleteTo">The key of the template to use when generating the delete template. The default is "default".</param>
        /// <param name="keyOfNewlyGeneratedDeleteTemplate">The key of the newly generated template. The default is "delete".</param>
        /// <returns>The given resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static FormsResource AddDelete(this FormsResource resource, string keyOfTemplateToAddDeleteTo = "default", string keyOfNewlyGeneratedDeleteTemplate = "delete")
        {
            ArgumentNullException.ThrowIfNull(resource);

            CopyTemplateWithoutProperties(resource.Templates, keyOfTemplateToAddDeleteTo, keyOfNewlyGeneratedDeleteTemplate, HttpMethods.Delete);

            return resource;
        }
        /// <summary>
        /// Adds a reload template to the given <see cref="FormsResource{T}"/>.
        /// </summary>
        /// <param name="resource">The resource to add the template to.</param>
        /// <param name="keyOfTemplateToAddDeleteTo">The key of the template to use when generating the delete template. The default is "default".</param>
        /// <param name="keyOfNewlyGeneratedDeleteTemplate">The key of the newly generated template. The default is "delete".</param>
        /// <returns>The given resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static FormsResource<T> AddDelete<T>(this FormsResource<T> resource, string keyOfTemplateToAddDeleteTo = "default", string keyOfNewlyGeneratedDeleteTemplate = "delete")
        {
            ArgumentNullException.ThrowIfNull(resource);

            CopyTemplateWithoutProperties(resource.Templates, keyOfTemplateToAddDeleteTo, keyOfNewlyGeneratedDeleteTemplate, HttpMethods.Delete);

            return resource;
        }

        /// <summary>
        /// Adds a reload template to the given <see cref="FormsResource"/>.
        /// </summary>
        /// <param name="resource">The resource to add the template to.</param>
        /// <param name="keyOfTemplateToAddReloadTo">The key of the template to use when generating the reload template. The default is "default".</param>
        /// <param name="keyOfNewlyGeneratedReloadTemplate">The key of the newly generated template. The default is "reload".</param>
        /// <returns>The given resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static FormsResource AddReload(this FormsResource resource, string keyOfTemplateToAddReloadTo = "default", string keyOfNewlyGeneratedReloadTemplate = "reload")
        {
            ArgumentNullException.ThrowIfNull(resource);

            CopyTemplateWithoutProperties(resource.Templates, keyOfTemplateToAddReloadTo, keyOfNewlyGeneratedReloadTemplate, HttpMethods.Get);

            return resource;
        }
        /// <summary>
        /// Adds a reload template to the given <see cref="FormsResource{T}"/>.
        /// </summary>
        /// <param name="resource">The resource to add the template to.</param>
        /// <param name="keyOfTemplateToAddReloadTo">The key of the template to use when generating the reload template. The default is "default".</param>
        /// <param name="keyOfNewlyGeneratedReloadTemplate">The key of the newly generated template. The default is "reload".</param>
        /// <returns>The given resource.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static FormsResource<T> AddReload<T>(this FormsResource<T> resource, string keyOfTemplateToAddReloadTo = "default", string keyOfNewlyGeneratedReloadTemplate = "reload")
        {
            ArgumentNullException.ThrowIfNull(resource);

            CopyTemplateWithoutProperties(resource.Templates, keyOfTemplateToAddReloadTo, keyOfNewlyGeneratedReloadTemplate, HttpMethods.Get);

            return resource;
        }

        private static void CopyTemplateWithoutProperties(IDictionary<string, FormTemplate> templates, string keyOfTemplateToCopy, string keyOfNewlyGeneratedTemplate, string method)
        {
            if (!templates.TryGetValue(keyOfTemplateToCopy, out var template))
                throw new ArgumentException($"The template with the key '{keyOfTemplateToCopy}' does not exist. Only templates with the keys {string.Join(", ", templates.Keys)} are present.", nameof(keyOfTemplateToCopy));

            if (templates.ContainsKey(keyOfNewlyGeneratedTemplate))
                new ArgumentException($"There is already a template present with the key {keyOfNewlyGeneratedTemplate}.", nameof(keyOfNewlyGeneratedTemplate));

            var reloadTemplate = new FormTemplate
            {
                Method = method,
                Target = template.Target,
                Title = template.Title
            };

            templates[keyOfNewlyGeneratedTemplate] = reloadTemplate;
        }
    }
}
