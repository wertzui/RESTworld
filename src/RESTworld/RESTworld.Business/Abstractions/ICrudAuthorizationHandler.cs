using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTworld.Business.Abstractions
{
    public interface ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult);

        Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse);

        Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult);

        Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse);

        Task<AuthorizationResult<TEntity, Func<IQueryable<TEntity>, IQueryable<TEntity>>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, Func<IQueryable<TEntity>, IQueryable<TEntity>>> previousResult);

        Task<ServiceResponse<IReadOnlyCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyCollection<TGetListDto>> previousResponse);

        Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult);

        Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse);

        Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult);

        Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse);
    }
}