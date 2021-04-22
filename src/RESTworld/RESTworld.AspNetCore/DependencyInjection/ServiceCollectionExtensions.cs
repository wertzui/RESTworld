using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESTworld.AspNetCore;
using RESTworld.AspNetCore.Controller;
using RESTworld.Business;
using RESTworld.Business.Abstractions;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using RESTworld.Common.Dtos;
using System.Linq;
using System.Reflection;
using RESTworld.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextFactoryWithDefaults<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            services.AddDbContextFactory<TContext>(builder =>
                builder
                    .UseSqlServer(
                        configuration.GetConnectionString(typeof(TContext).Name),
                        optionsBuilder =>
                        {
                            optionsBuilder.EnableRetryOnFailure();
                        })
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging());

            services.AddHealthChecks().AddDbContextCheck<TContext>();

            return services;
        }

        public static IServiceCollection AddRestPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
        {
            services.AddScoped<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
            CrudControllerFeatureProvider.AddController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

            return services;
        }

        public static IServiceCollection AddRestPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>(this IServiceCollection services)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TService: class, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            services.AddScoped<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TService>();
            CrudControllerFeatureProvider.AddController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

            return services;
        }

        public static IServiceCollection AddRestPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TAuthorizationhandler>(this IServiceCollection services)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            services.AddRestPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();
            services.AddAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

            return services;
        }

        public static IServiceCollection AddRestPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService, TAuthorizationhandler>(this IServiceCollection services)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TService : class, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            services.AddRestPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();
            services.AddAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();

            return services;
        }

        public static IServiceCollection AddODataModelForDbContext<TContext>(this IServiceCollection services)
        {
            var dbSetType = typeof(DbSet<>);
            var entityTypes = typeof(TContext).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.PropertyType)
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == dbSetType)
                .Select(t => t.GenericTypeArguments[0]);

            foreach (var entityType in entityTypes)
            {
                StartupBase.ODataModelBuilder.AddEntitySet(entityType.Name, new AspNet.OData.Builder.EntityTypeConfiguration(StartupBase.ODataModelBuilder, entityType));
            }

            return services;
        }

        public static IServiceCollection AddAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services)
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
        {
            services.AddScoped<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TAuthorizationhandler>();

            return services;
        }

        public static IServiceCollection AddUserAccessor(this IServiceCollection services) => services.AddScoped<IUserAccessor, UserAccessor>();
    }
}