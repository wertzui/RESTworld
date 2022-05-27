using Microsoft.Extensions.DependencyInjection;
using RESTworld.Business.Services;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System;

namespace RESTworld.Testing
{
    /// <summary>
    /// Contains extension methods related to <see cref="ServiceTestConfiguration{TService, TImplementation}"/>.
    /// </summary>
    public static class ServiceTestConfigurationExtensions
    {
        /// <summary>
        /// Creates the test environment using the last added <typeparamref name="TService"/> as SUT.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
        /// <param name="builder">The builder that contains the configured service.</param>
        /// <returns>An <see cref="ITestEnvironment{TService}"/>.</returns>
        public static ITestEnvironment<TService> BuildWithServiceAsSut<TService, TImplementation>(this ITestBuilderWithConfig<ServiceTestConfiguration<TService, TImplementation>> builder)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.Build<TService>();
        }

        /// <summary>
        /// Adds an <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>> WithCrudService<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>(this ITestBuilder builder)
            where TContext : DbContextBase
            where TEntity : ConcurrentEntityBase
            where TGetListDto : ConcurrentDtoBase
            where TGetFullDto : ConcurrentDtoBase
            where TUpdateDto : ConcurrentDtoBase
        {
            return builder.WithService<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
        }

        /// <summary>
        /// Adds an <see cref="ICrudServiceBase{TEntity, TGetFullDto, TGetFullDto, TGetFullDto, TGetFullDto}"/> to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<ICrudServiceBase<TEntity, TGetFullDto, TGetFullDto, TGetFullDto, TGetFullDto>, CrudServiceBase<TContext, TEntity, TGetFullDto, TGetFullDto, TGetFullDto, TGetFullDto>>> WithCrudService<TContext, TEntity, TGetFullDto>(this ITestBuilder builder)
            where TContext : DbContextBase
            where TEntity : ConcurrentEntityBase
            where TGetFullDto : ConcurrentDtoBase
        {
            return builder.WithCrudService<TContext, TEntity, TGetFullDto, TGetFullDto, TGetFullDto, TGetFullDto>();
        }

        /// <summary>
        /// Adds an <see cref="ICrudServiceBase{TEntity, TGetFullDto, TGetListDto, TGetFullDto, TGetFullDto}"/> to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<ICrudServiceBase<TEntity, TGetFullDto, TGetListDto, TGetFullDto, TGetFullDto>, CrudServiceBase<TContext, TEntity, TGetFullDto, TGetListDto, TGetFullDto, TGetFullDto>>> WithCrudService<TContext, TEntity, TGetListDto, TGetFullDto>(this ITestBuilder builder)
            where TContext : DbContextBase
            where TEntity : ConcurrentEntityBase
            where TGetListDto : ConcurrentDtoBase
            where TGetFullDto : ConcurrentDtoBase
        {
            return builder.WithCrudService<TContext, TEntity, TGetFullDto, TGetListDto, TGetFullDto, TGetFullDto>();
        }

        /// <summary>
        /// Adds an <see cref="IReadServiceBase{TEntity, TGetListDto, TGetFullDto}"/> to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<IReadServiceBase<TEntity, TGetListDto, TGetFullDto>, ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>>> WithReadService<TContext, TEntity, TGetListDto, TGetFullDto>(this ITestBuilder builder)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetListDto : DtoBase
            where TGetFullDto : DtoBase
        {
            return builder.WithService<IReadServiceBase<TEntity, TGetListDto, TGetFullDto>, ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>>();
        }

        /// <summary>
        /// Adds an <see cref="IReadServiceBase{TEntity, TGetFullDto, TGetFullDto}"/> to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<IReadServiceBase<TEntity, TGetFullDto, TGetFullDto>, ReadServiceBase<TContext, TEntity, TGetFullDto, TGetFullDto>>> WithReadService<TContext, TEntity, TGetFullDto>(this ITestBuilder builder)
            where TContext : DbContextBase
            where TEntity : EntityBase
            where TGetFullDto : DtoBase
        {
            return builder.WithReadService<TContext, TEntity, TGetFullDto, TGetFullDto>();
        }

        /// <summary>
        /// Adds a service to your tests.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<TService, TImplementation>> WithService<TService, TImplementation>(this ITestBuilder builder)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.With(new ServiceTestConfiguration<TService, TImplementation>());
        }

        /// <summary>
        /// Adds a service to your tests.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<TImplementation, TImplementation>> WithService<TImplementation>(this ITestBuilder builder)
            where TImplementation : class
        {
            return builder.With(new ServiceTestConfiguration<TImplementation, TImplementation>());
        }

        /// <summary>
        /// Adds a service to your tests.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="instance">The singleton instance to use.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<TService, TImplementation>> WithService<TService, TImplementation>(this ITestBuilder builder, TImplementation instance)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.With(new ServiceTestConfiguration<TService, TImplementation>(instance));
        }

        /// <summary>
        /// Adds a service to your tests.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the service implementation.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="instance">The singleton instance to use.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<TImplementation, TImplementation>> WithService<TImplementation>(this ITestBuilder builder, TImplementation instance)
            where TImplementation : class
        {
            return builder.With(new ServiceTestConfiguration<TImplementation, TImplementation>(instance));
        }
    }

    /// <summary>
    /// This test configuration can add any service to your tests.
    /// </summary>
    public class ServiceTestConfiguration<TService, TImplementation> : ITestConfiguration
        where TService : class
        where TImplementation : class, TService
    {
        private TImplementation? _instance;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ServiceTestConfiguration{TService, TImplementation}"/> class.
        /// </summary>
        public ServiceTestConfiguration()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ServiceTestConfiguration{TService, TImplementation}"/> class.
        /// </summary>
        /// <param name="instance">The singleton instance to use.</param>
        public ServiceTestConfiguration(TImplementation instance)
        {
            _instance = instance;
        }

        /// <inheritdoc/>
        public void AfterConfigureServices(IServiceProvider provider)
        {
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            if (_instance is not null)
                services.AddSingleton<TService>(_instance);
            else
                services.AddSingleton<TService, TImplementation>();
        }
    }
}