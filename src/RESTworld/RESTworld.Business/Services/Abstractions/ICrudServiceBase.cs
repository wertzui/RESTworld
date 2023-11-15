using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Services.Abstractions;

/// <summary>
/// Defines the base interface for all CRUD services.
/// If you want to implement your own service, you must implement this interface.
/// However it might be easier if you implement the CrudServiceBase class and just add or override specific methods.
/// </summary>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
/// <typeparam name="TCreateDto">The type of the DTO that is used for CREATE operations.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO that is used for READ-list operations.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO that is used for READ-single operations.</typeparam>
/// <typeparam name="TUpdateDto">The type of the DTO that is used for UPDATE operations.</typeparam>
public interface ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
    where TEntity : ConcurrentEntityBase
    where TGetListDto : DtoBase
    where TGetFullDto : ConcurrentDtoBase
    where TUpdateDto : ConcurrentDtoBase
{
    /// <summary>
    /// Creates the given DTO as an entity in the database.
    /// </summary>
    /// <param name="dto">The type of the DTO.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The DTO as it is stored in the database.</returns>
    Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the given DTOs as an entities in the database.
    /// </summary>
    /// <param name="dtos">The type of the DTOs.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The DTOs as they are stored in the database.</returns>
    Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateAsync(IReadOnlyCollection<TCreateDto> dtos, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the entity with the given <paramref name="id"/> and <paramref name="timestamp"/> from the database.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="timestamp">The current timestamp of the entity to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>An empty ServiceResponse, stating success or failure.</returns>
    Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the entity with new properties as defined in the given DTO.
    /// </summary>
    /// <param name="dto">The new properties which are used to update the existing entity in the database.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The DTO as it is stored in the database.</returns>
    Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the entities with new properties as defined in the given DTOs.
    /// </summary>
    /// <param name="request">Specifies the DTOs to update and optional filtering.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The DTOs as they are stored in the database.</returns>
    Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateAsync(IUpdateMultipleRequest<TUpdateDto, TEntity> request, CancellationToken cancellationToken);
}