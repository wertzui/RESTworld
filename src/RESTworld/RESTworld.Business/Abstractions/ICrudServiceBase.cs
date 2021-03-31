using RESTworld.EntityFrameworkCore.Models;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTworld.Business.Abstractions
{
    public interface ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
        Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto);

        Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp);

        Task<ServiceResponse<IReadOnlyCollection<TGetListDto>>> GetListAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter);

        Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id);

        Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto);
    }
}