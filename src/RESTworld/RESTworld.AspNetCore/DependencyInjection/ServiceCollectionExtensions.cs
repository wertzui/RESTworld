using HAL.AspNetCore.Forms.Abstractions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RESTworld.AspNetCore;
using RESTworld.AspNetCore.Authorization;
using RESTworld.AspNetCore.Forms;
using RESTworld.AspNetCore.Health;
using RESTworld.AspNetCore.HostedServices;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services.Abstractions;
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
            where TEntity : ConcurrentEntityBase
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