using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.Business.Mapping.AutoMapper;

namespace RESTworld.AspNetCore.DependencyInjection;

public static class MapperExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>()
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
            => services.AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, ReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>>();

        public IServiceCollection AddCrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>()
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
            => services.AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, CrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>>();
    }

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>()
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
        {
            builder.Services.AddReadAutoMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>();

            return builder;
        }

        public IHostApplicationBuilder AddCrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>()
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
        {
            builder.Services.AddCrudAutoMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>();

            return builder;
        }
    }
}
