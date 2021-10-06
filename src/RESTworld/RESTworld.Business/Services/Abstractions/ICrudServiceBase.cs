using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTworld.Business.Services.Abstractions
{
    /// <summary>
    /// Defines the base interface for all CRUD services.
    /// If you want to implement your own service, you must implement this interface.
    /// However it might be easier if you implement the CrudServiceBase class and just add or override specific methods.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TCreateDto"></typeparam>
    /// <typeparam name="TGetListDto"></typeparam>
    /// <typeparam name="TGetFullDto"></typeparam>
    /// <typeparam name="TUpdateDto"></typeparam>
    public interface ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
        /// <summary>
        /// Creates the given DTO as an entity in the database.
        /// </summary>
        /// <param name="dto">The type of the DTO.</param>
        /// <returns>The DTO as it is stored in the database.</returns>
        Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto);

        /// <summary>
        /// Creates the given DTOs as an entities in the database.
        /// </summary>
        /// <param name="dtos">The type of the DTOs.</param>
        /// <returns>The DTOs as they are stored in the database.</returns>
        Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateAsync(IReadOnlyCollection<TCreateDto> dtos);

        /// <summary>
        /// Deletes the entity with the given <paramref name="id"/> and <paramref name="timestamp"/> from the database.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <param name="timestamp">The current timestamp of the entity to delete.</param>
        /// <returns>An empty ServiceResponse, stating success or failure.</returns>
        Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp);

        /// <summary>
        /// Gets multiple entries from the database, filtered and paged as defined in the <paramref name="request"/>.
        /// Depending on the DTO, the result might not have all possible properties which are returned when <see cref="GetSingleAsync(long)"/> is called.
        /// </summary>
        /// <param name="request">Specifies filtering, paging and ordering through its Filter and if the total count shall be returned.</param>
        /// <returns></returns>
        Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListAsync(IGetListRequest<TEntity> request);

        /// <summary>
        /// Gets a single entry, defined by the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>The DTO as it is stored in the database.</returns>
        Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id);

        /// <summary>
        /// Updates the entity with new properties as defined in the given DTO.
        /// </summary>
        /// <param name="dto">The new properties which are used to update the existing entity in the database.</param>
        /// <returns>The DTO as it is stored in the database.</returns>
        Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto);

        /// <summary>
        /// Updates the entities with new properties as defined in the given DTOs.
        /// </summary>
        /// <param name="request">Specifies the DTOs to update and optional filtering.</param>
        /// <returns>The DTOs as they are stored in the database.</returns>
        Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateAsync(IUpdateMultipleRequest<TUpdateDto, TEntity> request);
    }
}