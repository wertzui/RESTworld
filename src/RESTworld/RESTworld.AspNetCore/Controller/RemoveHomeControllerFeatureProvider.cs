using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller;

/// <summary>
/// This feature provide is responsible for removing the <see cref="HomeController"/> to the controller list.
/// Use it if you do not want the default Home Controller.
/// </summary>
public class RemoveHomeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    /// <inheritdoc/>
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var type = typeof(HomeController).GetTypeInfo();
        feature.Controllers.Remove(type);
    }
}