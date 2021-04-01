using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// Used to set the controller name for routing purposes.Without this convention the
    /// names would be like 'CrudControllerBase`6[Widget]' instead of 'Widget'.
    /// Conventions can be applied as attributes or added to MvcOptions.Conventions.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <seealso cref="IControllerModelConvention" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CrudControllerNameConventionAttribute : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (!controller.ControllerType.IsGenericType ||
                controller.ControllerType.GetGenericTypeDefinition() != typeof(CrudController<,,,,>))
            {
                // Not a CrudControllerBase, ignore.
                return;
            }

            var readDtoType = controller.ControllerType.GenericTypeArguments[2];
            controller.ControllerName = readDtoType.Name;

            if (controller.ControllerName.StartsWith("tbl", StringComparison.OrdinalIgnoreCase))
                controller.ControllerName = controller.ControllerName[3..];

            if (controller.ControllerName.EndsWith("dto", StringComparison.OrdinalIgnoreCase))
                controller.ControllerName = controller.ControllerName[..^3];
        }
    }
}