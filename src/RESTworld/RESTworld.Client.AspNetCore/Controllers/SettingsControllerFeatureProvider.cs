using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RESTworld.Client.AspNetCore.Controllers
{
    /// <summary>
    /// This feature provide is responsible for adding the <see cref="SettingsController"/> to the controller list.
    /// </summary>
    public class SettingsControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        /// <inheritdoc/>
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            // Resolve controller name to avoid duplicates.
            var type = typeof(SettingsController).GetTypeInfo();
            var controllerModel = new ControllerModel(type, type.GetCustomAttributes().ToList());

            foreach (var controllerModelConvention in controllerModel.Attributes.OfType<IControllerModelConvention>())
            {
                controllerModelConvention.Apply(controllerModel);
            }

            var controllerName = nameof(SettingsController);

            // If a controller with the same name already exists, do not register the Settings controller from RESTworld.
            if (feature.Controllers.Any(c => c.Name == controllerName))
                return;

            // Add generic controller.
            feature.Controllers.Add(type);
        }
    }
}