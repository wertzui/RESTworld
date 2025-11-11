using HAL.AspNetCore.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using RESTworld.Common.Dtos;

namespace RESTworld.AspNetCore.Links.Abstractions;

/// <summary>
/// A factory which can ad links to resources for CRUD operations.
/// </summary>
public interface ICrudLinkFactory : ILinkFactory
{
    /// <summary>
    /// Adds a delete link to the given resource if the state of the resource is a <see cref="ConcurrentDtoBase"/>.
    /// </summary>
    /// <typeparam name="TDto">The type of the resource.</typeparam>
    /// <param name="resource">The resource to add the link to.</param>
    Resource<TDto> AddDeleteLink<TDto>(Resource<TDto> resource) where TDto : DtoBase?;

    /// <summary>
    /// Adds a delete link to the given resource.
    /// </summary>
    /// <param name="resource">The resource to add the link to.</param>
    TFormsResource AddDeleteLink<TFormsResource>(TFormsResource resource) where TFormsResource : FormsResource;

    /// <summary>
    /// Adds a history link to the given resource.
    /// </summary>
    /// <param name="resource">The resource to add the link to.</param>
    /// <typeparam name="TDto">The type of the resources state.</typeparam>
    Resource<TDto> AddHistoryLink<TDto>(Resource<TDto> resource) where TDto : DtoBase?;

    /// <summary>
    /// Adds a history link to the given resource.
    /// </summary>
    /// <param name="resource">The resource to add the link to.</param>
    /// <param name="dto">The DTO to get the history for.</param>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <typeparam name="TDto">The type of the resources state.</typeparam>
    TResource AddHistoryLink<TResource, TDto>(TResource resource, TDto dto)
        where TResource : Resource
        where TDto : DtoBase?;

    /// <summary>
    /// Adds the "new" link to the resource which is used to retrieve a template for creating
    /// new entries.
    /// </summary>
    /// <param name="resource">The resource to add the link to.</param>
    TResource AddNewLink<TResource>(TResource resource) where TResource : Resource;

    /// <summary>
    /// Adds a save link to the given resource. Adds a delete link to the given resource if the
    /// state is a <see cref="ConcurrentDtoBase"/>.
    /// </summary>
    /// <typeparam name="TDto">The type of the resource.</typeparam>
    /// <param name="resource">The resource to add the link to.</param>
    Resource<TDto> AddSaveAndDeleteLinks<TDto>(Resource<TDto> resource) where TDto : DtoBase?;

    /// <summary>
    /// Adds a save link to the given resource.
    /// </summary>
    /// <typeparam name="TDto">The type of the resource.</typeparam>
    /// <param name="resource">The resource to add the link to.</param>
    Resource<TDto> AddSaveLink<TDto>(Resource<TDto> resource) where TDto : DtoBase?;
}