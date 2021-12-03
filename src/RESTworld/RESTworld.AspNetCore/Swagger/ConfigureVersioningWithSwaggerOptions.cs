using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace RESTworld.AspNetCore.Swagger
{
    /// <summary>
    /// Configures Swagger to work with API versioning.
    /// </summary>
    /// <seealso cref="IConfigureOptions{SwaggerGenOptions}" />
    public class ConfigureVersioningWithSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureVersioningWithSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public ConfigureVersioningWithSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            this.provider = provider;
        }

        /// <inheritdoc/>
        public void Configure(SwaggerGenOptions options)
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                  description.GroupName,
                    new OpenApiInfo()
                    {
                        Title = $"{assemblyName} {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                    });
            }
        }
    }
}