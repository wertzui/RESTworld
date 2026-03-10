using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Controller;
using RESTworld.Business.Mapping;
using RESTworld.Business.Mapping.AutoMapper;

namespace RESTworld.AspNetCore.DependencyInjection;

public static class MapperExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, TMapper>()
            where TMapper : class, IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
        {
            services.AddSingleton<TMapper>();
            services.AddSingleton<IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>, TMapper>();
            services.AddSingleton<IListRequestFactory<TEntity, TQueryDto, TGetListDto, TGetFullDto>, ListRequestFactory<TEntity, TQueryDto, TGetListDto, TGetFullDto>>();

            return services;
        }

        public IServiceCollection AddCreateMapper<TEntity, TCreateDto, TMapper>()
            where TMapper : class, ICreateMapper<TEntity, TCreateDto>
        {
            services.AddSingleton<TMapper>();
            services.AddSingleton<ICreateMapper<TEntity, TCreateDto>, TMapper>();

            return services;
        }

        public IServiceCollection AddUpdateMapper<TEntity, TUpdateDto, TMapper>()
            where TMapper : class, IUpdateMapper<TEntity, TUpdateDto>
        {
            services.AddSingleton<TMapper>();
            services.AddSingleton<IUpdateMapper<TEntity, TUpdateDto>, TMapper>();

            return services;
        }

        public IServiceCollection AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TMapper>()
            where TMapper : class, ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
        {
            services.AddCreateMapper<TEntity, TCreateDto, TMapper>();
            services.AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, TMapper>();
            services.AddUpdateMapper<TEntity, TUpdateDto, TMapper>();
            services.AddSingleton<ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>, TMapper>();

            return services;
        }

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
        public IHostApplicationBuilder AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, TMapper>()
            where TMapper : class, IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto>
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
        {
            builder.Services.AddReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto, TMapper>();

            return builder;
        }

        public IHostApplicationBuilder AddCreateMapper<TEntity, TCreateDto, TMapper>()
            where TMapper : class, ICreateMapper<TEntity, TCreateDto>
        {
            builder.Services.AddCreateMapper<TEntity, TCreateDto, TMapper>();

            return builder;
        }

        public IHostApplicationBuilder AddUpdateMapper<TEntity, TUpdateDto, TMapper>()
            where TMapper : class, IUpdateMapper<TEntity, TUpdateDto>
        {
            builder.Services.AddUpdateMapper<TEntity, TUpdateDto, TMapper>();

            return builder;
        }

        public IHostApplicationBuilder AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TMapper>()
            where TMapper : class, ICrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TEntity : class
            where TQueryDto : class
            where TGetListDto : class
            where TGetFullDto : class
        {
            builder.Services.AddCrudMapper<TEntity, TCreateDto, TQueryDto, TGetListDto, TGetFullDto, TUpdateDto, TMapper>();

            return builder;
        }

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
