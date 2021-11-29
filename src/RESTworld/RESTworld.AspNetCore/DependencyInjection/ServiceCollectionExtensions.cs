using HAL.AspNetCore.Forms.Abstractions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RESTworld.AspNetCore;
using RESTworld.AspNetCore.Authorization;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Forms;
using RESTworld.AspNetCore.Health;
using RESTworld.AspNetCore.HostedServices;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="IBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> which you
        /// can use when calling <see cref="ServiceBase.TryExecuteWithAuthorizationAsync{TEntity,
        /// T1, TResponse, TAuthorizationHandler}(T1,
        /// System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1},
        /// System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}},
        /// System.Func{RESTworld.Business.Authorization.AuthorizationResult{TEntity, T1},
        /// TAuthorizationHandler,
        /// System.Threading.Tasks.Task{RESTworld.Business.Authorization.AuthorizationResult{TEntity,
        /// T1}}}, System.Func{RESTworld.Business.Models.ServiceResponse{TResponse},
        /// TAuthorizationHandler,
        /// System.Threading.Tasks.Task{RESTworld.Business.Models.ServiceResponse{TResponse}}},
        /// System.Collections.Generic.IEnumerable{TAuthorizationHandler})"/> from your service method.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBasicAuthorizationHandler<TAuthorizationhandler, TEntity, TRequest, TResponse>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
            where TEntity : EntityBase
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<IBasicAuthorizationHandler<TEntity, TRequest, TResponse>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }

        /// <summary>
        /// Adds an <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/> which will automatically be called by any <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
        /// </summary>
        /// <typeparam name="TAuthorizationhandler">The type of the authorization handler.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration instance which holds the RESTWorld configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddCrudAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services, IConfiguration configuration)
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
        {
            if (!configuration.GetValue<bool>($"{nameof(RESTworld)}:{nameof(RestWorldOptions.DisableAuthorization)}"))
            {
                services.AddScoped<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TAuthorizationhandler>();
                services.AddScoped<TAuthorizationhandler>();
            }

            return services;
        }

        /// <summary>
        /// Adds a custom service with basic authorization, using a custom <typeparamref
        /// name="TService"/> and the <typeparamref name="TAuthorizationhandler"/>. Note that you
        /// need to write the custom controller which will call your service yourself and put it
        /// into the /Controllers folder of your application so it gets automatically recognized.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TService">
        /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/> implementation.
        /// </typeparam>
        /// <typeparam name="TAuthorizationhandler">
        /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/>.
        /// </typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddCustomServiceAndAuthorization<TEntity, TRequest, TResponse, TService, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration)
            where TEntity : EntityBase
            where TService : class
            where TAuthorizationhandler : class, IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
        {
            services.AddScoped<TService>();
            services.AddBasicAuthorizationHandler<TAuthorizationhandler, TEntity, TRequest, TResponse>(configuration);

            return services;
        }

        /// <summary>
        /// Adds a pooled <see cref="IDbContextFactory{TContext}"/> to the service collection. The
        /// connection string comes from the configuration section "ConnectionStrings" with the name
        /// of the context type.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
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

            var contextType = typeof(TContext);
            var contextName = contextType.Name;

            services.AddPooledDbContextFactory<TContext>(builder =>
                builder
                    .UseSqlServer(
                        configuration.GetConnectionString(contextName),
                        optionsBuilder =>
                        {
                            optionsBuilder.EnableRetryOnFailure();
                        })
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging());

            services.AddHealthChecks().AddCheck<DbContextFactoryMigrationHealthCheck<TContext>>(contextName + "Migration", tags: new[] { "startup" });
            services.AddHealthChecks().AddCheck<DbContextFactoryConnectionHealthCheck<TContext>>(contextName + "Connection", tags: new[] { "ready" });

            return services;
        }

        /// <summary>
        /// Adds a factory to generate foreign keys to the CRUD controller endpoint of the given
        /// List DTO.
        /// </summary>
        /// <typeparam name="TListDto">The type of the List DTO.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddForeignKeyForFormTo<TListDto>(this IServiceCollection services)
            => services.AddScoped<IForeignKeyLinkFactory, CrudForeignKeyLinkFactory<TListDto>>();

        /// <summary>
        /// Adds an ODataModel for a <see cref="DbContext"/>. This is required for List operations
        /// to work.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddODataModelForDbContext<TContext>(this IServiceCollection services)
        {
            var dbSetType = typeof(DbSet<>);
            var entityTypes = typeof(TContext).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.PropertyType)
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == dbSetType)
                .Select(t => t.GenericTypeArguments[0]);

            foreach (var entityType in entityTypes)
            {
                StartupBase.ODataModelBuilder.AddEntitySet(entityType.Name, new EntityTypeConfiguration(StartupBase.ODataModelBuilder, entityType));
            }

            return services;
        }

        /// <summary>
        /// Adds a complete REST pipeline without authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and
        /// the <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="apiVersion">An optional API version.</param>
        /// <param name="isDeprecated">
        /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddRestPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this IServiceCollection services, ApiVersion apiVersion = null, bool isDeprecated = false)
                    where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
        {
            services.AddScoped<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
            services.AddForeignKeyForFormTo<TGetListDto>();
            CrudControllerFeatureProvider.AddController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();
            services.Configure<ApiVersioningOptions>(options =>
            {
                var controllerConvention = options.Conventions.Controller<CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
                if (apiVersion is null)
                {
                    controllerConvention.IsApiVersionNeutral();
                }
                else
                {
                    if (isDeprecated)
                        controllerConvention.HasDeprecatedApiVersion(apiVersion);
                    else
                        controllerConvention.HasApiVersion(apiVersion);
                }
            });

            return services;
        }

        /// <summary>
        /// Adds a complete REST pipeline with authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>, the
        /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto,
        /// TUpdateDto}"/> and the <typeparamref name="TAuthorizationhandler"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
        /// <typeparam name="TAuthorizationhandler">
        /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/>.
        /// </typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
        /// </param>
        /// <param name="apiVersion">An optional API version.</param>
        /// <param name="isDeprecated">
        /// if set to <c>true</c> the pipeline with this version is treated as deprecated.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddRestPipelineWithAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration, ApiVersion apiVersion = null, bool isDeprecated = false)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            services.AddRestPipeline<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(apiVersion, isDeprecated);
            services.AddCrudAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(configuration);

            return services;
        }

        /// <summary>
        /// Adds a complete REST pipeline without authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and a
        /// custom <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
        /// <typeparam name="TService">
        /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/> implementation.
        /// </typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddRestPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>(this IServiceCollection services)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TService : class, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            services.AddScoped<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, TService>();
            CrudControllerFeatureProvider.AddController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>();
            services.AddForeignKeyForFormTo<TGetListDto>();

            return services;
        }

        /// <summary>
        /// Adds a complete REST pipeline with authorization, using the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>, a
        /// custom <typeparamref name="TService"/> and the <typeparamref name="TAuthorizationhandler"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
        /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
        /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
        /// <typeparam name="TService">
        /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/> implementation.
        /// </typeparam>
        /// <typeparam name="TAuthorizationhandler">
        /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
        /// TGetFullDto, TUpdateDto}"/>.
        /// </typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> instance which holds the RESTWorld configuration.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddRestPipelineWithCustomServiceAndAuthorization<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService, TAuthorizationhandler>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
            where TUpdateDto : DtoBase
            where TService : class, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
            where TAuthorizationhandler : class, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        {
            services.AddRestPipelineWithCustomService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto, TService>();
            services.AddCrudAuthorizationHandler<TAuthorizationhandler, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(configuration);

            return services;
        }

        /// <summary>
        /// Adds and <see cref="IUserAccessor"/> to the services which can be used to retrieve the
        /// current user from the HttpContext.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddUserAccessor(this IServiceCollection services) => services.AddScoped<IUserAccessor, UserAccessor>();

        /// <summary>
        /// Adds the database to the list of databases to migrate to the latest version during startup.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the database context.</typeparam>
        /// <param name="services">The services.</param>
        public static void MigrateDatabaseDuringStartup<TDbContext>(this IServiceCollection services)
            where TDbContext : DbContext
        {
            services.AddHostedService<DatabaseMigrationHostedService<TDbContext>>();
        }
    }
}