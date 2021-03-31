using System;

namespace RESTworld.AspNetCore.Swagger
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SwaggerIgnoreAttribute : Attribute
    {
    }
}