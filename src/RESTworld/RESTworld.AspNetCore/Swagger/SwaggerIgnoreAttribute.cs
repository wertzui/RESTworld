using System;

namespace RESTworld.AspNetCore.Swagger
{
    /// <summary>
    /// Tells swagger to ignore this parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SwaggerIgnoreAttribute : Attribute
    {
    }
}