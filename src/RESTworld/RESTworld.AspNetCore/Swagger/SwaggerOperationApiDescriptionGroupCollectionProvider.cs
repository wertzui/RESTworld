using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RESTworld.AspNetCore.Swagger
{
    /// <summary>
    /// This class is used to provide an <see cref="IApiDescriptionGroupCollectionProvider"/> in the <see cref="SwaggerExampleOperationFilter"/>.
    /// </summary>
    /// <seealso cref="IApiDescriptionGroupCollectionProvider" />
    public class SwaggerOperationApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerOperationApiDescriptionGroupCollectionProvider"/> class.
        /// </summary>
        /// <param name="operationFilterContext">The operation filter context.</param>
        public SwaggerOperationApiDescriptionGroupCollectionProvider(OperationFilterContext operationFilterContext)
        {
            ApiDescriptionGroups = new ApiDescriptionGroupCollection(new[] { new ApiDescriptionGroup(operationFilterContext.ApiDescription.GroupName, new[] { operationFilterContext.ApiDescription }) }, operationFilterContext.ApiDescription.GetApiVersion().MajorVersion.GetValueOrDefault());
        }

        /// <inheritdoc/>
        public ApiDescriptionGroupCollection ApiDescriptionGroups { get; }
    }
}