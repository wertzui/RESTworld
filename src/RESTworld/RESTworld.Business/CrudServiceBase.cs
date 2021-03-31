using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Abstractions;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RESTworld.Business
{
    public class CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        : ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
        protected readonly IDbContextFactory<TContext> _contextFactory;
        protected readonly ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> _logger;
        protected readonly IMapper _mapper;

        public CrudServiceBase(IDbContextFactory<TContext> contextFactory, IMapper mapper,
            ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto)
            => TryExecuteAsync(() => CreateInternalAsync(dto));

        public Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp)
            => TryExecuteAsync(() => DeleteInternalAsync(id, timestamp));

        public Task<ServiceResponse<IReadOnlyCollection<TGetListDto>>> GetListAsync(Func<System.Linq.IQueryable<TEntity>, System.Linq.IQueryable<TEntity>> filter)
            => TryExecuteAsync(() => GetListInternalAsync(filter));

        public Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id)
            => TryExecuteAsync(() => GetSingleInternalAsync(id));

        public Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto)
            => TryExecuteAsync(() => UpdateInternalAsync(dto));

        protected virtual async Task<ServiceResponse<TGetFullDto>> CreateInternalAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            await using var context = _contextFactory.CreateDbContext();
            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(resultDto);
        }

        protected virtual async Task<ServiceResponse<object>> DeleteInternalAsync(long id, byte[] timestamp)
        {
            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<object>(HttpStatusCode.NotFound);

            if (!System.Linq.Enumerable.SequenceEqual(entity.Timestamp, timestamp))
                return ServiceResponse.FromProblem<object>(HttpStatusCode.Conflict, "The entity was modfied.");

            context.Remove(entity);

            await context.SaveChangesAsync();

            return ServiceResponse.FromStatus<object>(HttpStatusCode.OK);
        }

        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetListDto>>> GetListInternalAsync(Func<System.Linq.IQueryable<TEntity>, System.Linq.IQueryable<TEntity>> filter)
        {
            var set = _contextFactory.Set<TEntity>();

            if (filter is not null)
                set = filter(set);

            var entities = await set.ToListAsync();

            var dtos = _mapper.Map<IReadOnlyCollection<TGetListDto>>(entities);

            return ServiceResponse.FromResult(dtos);
        }

        protected virtual async Task<ServiceResponse<TGetFullDto>> GetSingleInternalAsync(long id)
        {
            var entity = await _contextFactory.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            var dto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(dto);
        }

        protected async Task<ServiceResponse<T>> TryExecuteAsync<T>(Func<Task<ServiceResponse<T>>> function)
        {
            try
            {
                return await function();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return ServiceResponse.FromException<T>(HttpStatusCode.Conflict, e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while executing a service call");
                return ServiceResponse.FromException<T>(e);
            }
        }

        protected virtual async Task<ServiceResponse<TGetFullDto>> UpdateInternalAsync(TUpdateDto dto)
        {
            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            context.Entry(entity).Property(nameof(EntityBase.Timestamp)).OriginalValue = dto.Timestamp;

            _mapper.Map(dto, entity);

            await context.SaveChangesAsync();

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(resultDto);
        }
    }
}