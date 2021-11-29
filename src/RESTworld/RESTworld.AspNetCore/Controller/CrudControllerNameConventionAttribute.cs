﻿using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
        /// <summary>
        /// Creates the controller name out of the given type.
        /// Normally you want to use the type that is used on a normal GET operation.
        /// It will try to strip all pre- and postfixes from the type.
        /// </summary>
        /// <typeparam name="TReadDto">The type of the DTO.</typeparam>
        /// <returns>The name of the type without any pre- or postfixes so it can be used as a controller name.</returns>
        public static string CreateNameFromType<TReadDto>() => CreateNameFromType(typeof(TReadDto));

        /// <summary>
        /// Creates the controller name out of the given type.
        /// Normally you want to use the type that is used on a normal GET operation.
        /// It will try to strip all pre- and postfixes from the type.
        /// </summary>
        /// <param name="readDtoType">The type of the DTO.</param>
        /// <returns>The name of the type without any pre- or postfixes so it can be used as a controller name.</returns>
        public static string CreateNameFromType(Type readDtoType)
        {
            var dtoName = readDtoType.Name;

            var match = Regex.Match(dtoName, "^(?<tbl>tbl)?(?<name>.*?)(?<get>get)?(?<full>full)?(?<list>list)?(?<dto>dto)?(?<version>v(er(sion)?)?\\d+)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var groups = match.Groups;

            var controllerName = groups["name"].Value;

            return controllerName;
        }

        /// <inheritdoc/>
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
    }
}