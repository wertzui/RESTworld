using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System.Threading.Tasks;

namespace RESTworld.Business.Services.Abstractions
{
    /// <summary>
    /// Defines the base interface for all READ-only services.
    /// If you want to implement your own read-only-service, you must implement this interface.
    /// However it might be easier if you implement the ReadServiceBase class and just add or override specific methods.
    /// </summary>
    /// <typeparam name="TEntity">The type of the database entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO that is used for READ-list operations.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO that is used for READ-single operations.</typeparam>
    public interface IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
    {

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
    }
}