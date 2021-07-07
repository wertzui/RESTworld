using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// Provides generic <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s to the ASP pipeline when they have been registered through one of the static methods.
    /// </summary>
    public class CrudControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private static readonly ICollection<TypeInfo> _controllerTypes = new List<TypeInfo>();

        /// <summary>
        /// Adds a <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> to the controllers.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TCreateDto"></typeparam>
        /// <typeparam name="TGetListDto"></typeparam>
        /// <typeparam name="TGetFullDto"></typeparam>
        /// <typeparam name="TUpdateDto"></typeparam>
        public static void AddController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>()
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
        => AddCustomController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();

        /// <summary>
        /// Adds a custom controller to the controllers.
        /// Note that if you place a Controller in the 'Controller' folder of you startup programm, ASP will automatically recognize it.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TCreateDto"></typeparam>
        /// <typeparam name="TGetListDto"></typeparam>
        /// <typeparam name="TGetFullDto"></typeparam>
        /// <typeparam name="TUpdateDto"></typeparam>
        /// <typeparam name="TController"></typeparam>
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

        /// <inheritdoc/>
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