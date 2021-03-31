using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using RESTworld.EntityFrameworkCore.Models;
using RESTworld.Common.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller
{
    public class CrudControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private static readonly ICollection<TypeInfo> _controllerTypes = new List<TypeInfo>();

        public static void AddController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>()
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
        {
            var controllerType = typeof(CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)
                .GetTypeInfo();

            _controllerTypes.Add(controllerType);
        }

        public static void AddCustomController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TController>()
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TController : CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            var controllerType = typeof(TController)
                .GetTypeInfo();

            _controllerTypes.Add(controllerType);
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var instanceControllerType in _controllerTypes)
            {
                // Resolve controller name to avoid duplicates.
                var controllerModel = new ControllerModel(instanceControllerType, instanceControllerType.GetCustomAttributes().ToList());

                foreach (var controllerModelConvention in controllerModel.Attributes.OfType<IControllerModelConvention>())
                {
                    controllerModelConvention.Apply(controllerModel);
                }
                var entityType = instanceControllerType.GenericTypeArguments.FirstOrDefault(typeof(EntityBase).IsAssignableFrom);

                controllerModel.Attributes.OfType<CrudControllerNameConventionAttribute>().Single().Apply(controllerModel);

                // If a controller with the same name already exists, do not register the generic one.
                if (feature.Controllers.Any(c => c.Name == controllerModel.ControllerName))
                    continue;

                // Add generic controller.
                feature.Controllers.Add(instanceControllerType);
            }
        }
    }
}