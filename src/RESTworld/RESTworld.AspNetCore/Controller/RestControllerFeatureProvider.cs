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
    public class RestControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private static readonly ICollection<TypeInfo> _controllerTypes = new List<TypeInfo>();

        /// <summary>
        /// Adds a <see cref="ReadController{TEntity, TGetListDto, TGetFullDto}"/> to the controllers.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TGetListDto"></typeparam>
        /// <typeparam name="TGetFullDto"></typeparam>
        public static void AddReadController<TEntity, TGetListDto, TGetFullDto>()
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
        => AddCustomReadController<TEntity, TGetListDto, TGetFullDto, ReadController<TEntity, TGetListDto, TGetFullDto>>();

        /// <summary>
        /// Adds a custom controller to the controllers.
        /// Note that if you place a Controller in the 'Controller' folder of you startup program, ASP will automatically recognize it.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TGetListDto"></typeparam>
        /// <typeparam name="TGetFullDto"></typeparam>
        /// <typeparam name="TController"></typeparam>
        public static void AddCustomReadController<TEntity, TGetListDto, TGetFullDto, TController>()
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TController : ReadController<TEntity, TGetListDto, TGetFullDto>
        {
            var controllerType = typeof(TController)
                .GetTypeInfo();

            _controllerTypes.Add(controllerType);
        }

        /// <summary>
        /// Adds a <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> to the controllers.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TCreateDto"></typeparam>
        /// <typeparam name="TGetListDto"></typeparam>
        /// <typeparam name="TGetFullDto"></typeparam>
        /// <typeparam name="TUpdateDto"></typeparam>
        public static void AddCrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>()
            where TEntity : ConcurrentEntityBase
            where TGetListDto : ConcurrentDtoBase
            where TGetFullDto : ConcurrentDtoBase
            where TUpdateDto : ConcurrentDtoBase
        => AddCustomCrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();

        /// <summary>
        /// Adds a custom controller to the controllers.
        /// Note that if you place a Controller in the 'Controller' folder of you startup program, ASP will automatically recognize it.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TCreateDto"></typeparam>
        /// <typeparam name="TGetListDto"></typeparam>
        /// <typeparam name="TGetFullDto"></typeparam>
        /// <typeparam name="TUpdateDto"></typeparam>
        /// <typeparam name="TController"></typeparam>
        public static void AddCustomCrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TController>()
            where TEntity : ConcurrentEntityBase
            where TGetListDto : ConcurrentDtoBase
            where TGetFullDto : ConcurrentDtoBase
            where TUpdateDto : ConcurrentDtoBase
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

                controllerModel.Attributes.OfType<RestControllerNameConventionAttribute>().Single().Apply(controllerModel);

                // If a controller with the same name already exists, do not register the generic one.
                if (feature.Controllers.Any(c => c.Name == controllerModel.ControllerName))
                    continue;

                // Add generic controller.
                feature.Controllers.Add(instanceControllerType);
            }
        }
    }
}