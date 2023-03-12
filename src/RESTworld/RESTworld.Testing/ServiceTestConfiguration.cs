using Microsoft.Extensions.DependencyInjection;
using RESTworld.Business.Services;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Business.Validation;
using RESTworld.Business.Validation.Abstractions;
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
        /// Adds the <see cref="IValidationService{TCreateDto, TUpdateDto, TEntity}"/> to your tests.
        /// DOes not add any validators.
        /// </summary>
        /// <param name="builder">The builder that contains the configured service.</param>
        /// <returns>An <see cref="ITestBuilderWithConfig{ServiceTestConfiguration}"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration> WithValidation(this ITestBuilder builder)
        {
            return builder.WithService(typeof(IValidationService<,,>), typeof(ValidationService<,,>));
        }

        /// <summary>
        /// Adds an <see cref="ICreateValidator{TCreateDto, TEntity}"/> to your tests.
        /// Automatically adds the <see cref="IValidationService{TCreateDto, TUpdateDto, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="builder">The builder that contains the configured service.</param>
        /// <param name="validator">The validator to add</param>
        /// <returns>An <see cref="ITestBuilderWithConfig{TConfig}"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<ICreateValidator<TCreateDto, TEntity>, ICreateValidator<TCreateDto, TEntity>>> WithValidation<TCreateDto, TEntity>(this ITestBuilder builder, ICreateValidator<TCreateDto, TEntity> validator)
        {
            return builder.WithValidation().WithService(validator);
        }

        /// <summary>
        /// Adds an <see cref="IUpdateValidator{TUpdateDto, TEntity}"/> to your tests.
        /// Automatically adds the <see cref="IValidationService{TUpdateDto, TUpdateDto, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TUpdateDto">The type of the Update DTO.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="builder">The builder that contains the configured service.</param>
        /// <param name="validator">The validator to add</param>
        /// <returns>An <see cref="ITestBuilderWithConfig{TConfig}"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<IUpdateValidator<TUpdateDto, TEntity>, IUpdateValidator<TUpdateDto, TEntity>>> WithValidation<TUpdateDto, TEntity>(this ITestBuilder builder, IUpdateValidator<TUpdateDto, TEntity> validator)
        {
            return builder.WithValidation().WithService(validator);
        }
        /// <summary>
        /// Adds an <see cref="IValidator{TCreateDto, TUpdateDto, TEntity}"/> to your tests.
        /// Automatically adds the <see cref="IValidationService{TCreateDto, TUpdateDto, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
        /// <typeparam name="TUpdateDto">The type of the Update DTO.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="builder">The builder that contains the configured service.</param>
        /// <param name="validator">The validator to add</param>
        /// <returns>An <see cref="ITestBuilderWithConfig{TConfig}"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration<IValidator<TCreateDto, TUpdateDto, TEntity>, IValidator<TCreateDto, TUpdateDto, TEntity>>> WithValidation<TCreateDto, TUpdateDto, TEntity>(this ITestBuilder builder, IValidator<TCreateDto, TUpdateDto, TEntity> validator)
        {
            return builder.WithValidation()
                .WithService<ICreateValidator<TCreateDto, TEntity>>(validator)
                .WithService<IUpdateValidator<TUpdateDto, TEntity>>(validator)
                .WithService(validator);
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
            return builder
                .WithService<ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>();
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
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="serviceType">The type of the service</param>
        /// <param name="implementationType">The type of the service implementation</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilderWithConfig<ServiceTestConfiguration> WithService(this ITestBuilder builder, Type serviceType, Type implementationType)
        {
            return builder.With(new ServiceTestConfiguration(serviceType, implementationType));
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
        private readonly TImplementation? _instance;

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

    /// <summary>
    /// This test configuration can add any service to your tests.
    /// </summary>
    public class ServiceTestConfiguration : ITestConfiguration
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceTestConfiguration"/> class.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationType">The type of the implementation.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServiceTestConfiguration(Type serviceType, Type implementationType)
        {
            _serviceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }


        /// <inheritdoc/>
        public void AfterConfigureServices(IServiceProvider provider)
        {
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_serviceType, _implementationType);
        }
    }
}