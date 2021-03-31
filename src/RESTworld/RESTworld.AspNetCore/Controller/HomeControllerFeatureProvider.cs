using guenstiger.Table.Controller;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller
{
    public class HomeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            // Resolve controller name to avoid duplicates.
            var type = typeof(HomeController).GetTypeInfo();
            var controllerModel = new ControllerModel(type, type.GetCustomAttributes().ToList());

            foreach (var controllerModelConvention in controllerModel.Attributes.OfType<IControllerModelConvention>())
            {
                controllerModelConvention.Apply(controllerModel);
            }

            var controllerName = nameof(HomeController);

            // If a controller with the same name already exists, do not register the home controller from RESTworld.
            if (feature.Controllers.Any(c => c.Name == controllerName))
                return;

            // Add generic controller.
            feature.Controllers.Add(type);
        }
    }
}