using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller;

/// <summary>
/// Provides generic <see cref="CrudController{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s to the ASP pipeline when they have been registered through one of the static methods.
/// </summary>
public class RestControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private static readonly ICollection<TypeInfo> _controllerTypes = new List<TypeInfo>();

    /// <summary>
    /// Adds a <see cref="ReadController{TEntity, TQueryDto, TGetListDto, TGetFullDto}"/> to the controllers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for queries on a get List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    public static void AddReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto>()
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
    => AddCustomReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto, ReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto>>();

    /// <summary>
    /// Adds a custom controller to the controllers.
    /// Note that if you place a Controller in the 'Controller' folder of you startup program, ASP will automatically recognize it.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for queries on a get List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    public static void AddCustomReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto, TController>()
        where TEntity : EntityBase
        where TQueryDto : class
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TController : ReadController<TEntity, TQueryDto, TGetListDto, TGetFullDto>
    {
        var controllerType = typeof(TController)
            .GetTypeInfo();

        _controllerTypes.Add(controllerType);
    }

    /// <summary>
    /// Adds a <see cref="CrudController{TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto}"/> to the controllers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for queries on a get List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    public static void AddCrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>()
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    => AddCustomCrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, CrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>>();

    /// <summary>
    /// Adds a custom controller to the controllers.
    /// Note that if you place a Controller in the 'Controller' folder of you startup program, ASP will automatically recognize it.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TQueryDto">The type of the DTO for queries on a get List operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    /// <typeparam name="TController">The type of the controller.</typeparam>
    public static void AddCustomCrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TController>()
        where TEntity : ConcurrentEntityBase
        where TQueryDto : class
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
        where TController : CrudController<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
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