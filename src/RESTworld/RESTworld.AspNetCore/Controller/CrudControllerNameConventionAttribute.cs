using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Text.RegularExpressions;

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

            var readDtoType = controller.ControllerType.GenericTypeArguments[3];
            controller.ControllerName = CreateNameFromType(readDtoType);
        }

        public static string CreateNameFromType<TReadDto>() => CreateNameFromType(typeof(TReadDto));

        public static string CreateNameFromType(Type readDtoType)
        {
            var name = readDtoType.Name;

            name = Regex.Match(name, "^(tbl)?(?<name>.*?)(get)?(full)?(dto)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Groups["name"].Value;

            return name;
        }
    }
}