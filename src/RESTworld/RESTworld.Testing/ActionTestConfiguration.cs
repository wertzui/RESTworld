using Microsoft.Extensions.DependencyInjection;
using System;

namespace RESTworld.Testing
{
    /// <summary>
    /// Contains extension methods related to <see cref="ActionTestConfiguration"/>.
    /// </summary>
    public static class ActionTestConfigurationExtensions
    {
        /// <summary>
        /// Adds the given service configuration to the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="configureServices">The method that is used to configure services.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithAction(this ITestBuilder builder, Action<IServiceCollection> configureServices)
        {
            return builder.With(new ActionTestConfiguration(configureServices));
        }

        /// <summary>
        /// Adds the given service configuration to the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="afterConfigureServices">
        /// The method that is called after the services have been configured.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithAction(this ITestBuilder builder, Action<IServiceProvider> afterConfigureServices)
        {
            return builder.With(new ActionTestConfiguration(afterConfigureServices));
        }

        /// <summary>
        /// Adds the given service configuration to the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="configureServices">The method that is used to configure services.</param>
        /// <param name="afterConfigureServices">
        /// The method that is called after the services have been configured.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithAction(this ITestBuilder builder, Action<IServiceCollection> configureServices, Action<IServiceProvider> afterConfigureServices)
        {
            return builder.With(new ActionTestConfiguration(configureServices, afterConfigureServices));
        }
    }

    /// <summary>
    /// A test configuration that can easily be used ad-hoc by injecting the configuration actions.
    /// This saves you from writing a complete class if you just need this functionality one time.
    /// </summary>
    public class ActionTestConfiguration : ITestConfiguration
    {
        private static readonly Action<IServiceProvider> EmptyAfterConfigureServices = _ => { };
        private static readonly Action<IServiceCollection> EmptyConfigureServices = _ => { };
        private readonly Action<IServiceProvider> _afterConfigureServices;
        private readonly Action<IServiceCollection> _configureServices;

        /// <summary>
        /// Creates a new instance of the <see cref="ActionTestConfiguration"/>
        /// </summary>
        /// <param name="configureServices">The action to execute while configuring the services.</param>
        /// <param name="afterConfigureServices">
        /// The action to execute after the services have been configured.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public ActionTestConfiguration(Action<IServiceCollection> configureServices, Action<IServiceProvider> afterConfigureServices)
        {
            _configureServices = configureServices ?? throw new ArgumentNullException(nameof(configureServices));
            _afterConfigureServices = afterConfigureServices ?? throw new ArgumentNullException(nameof(afterConfigureServices));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ActionTestConfiguration"/>
        /// </summary>
        /// <param name="configureServices">The action to execute while configuring the services.</param>
        public ActionTestConfiguration(Action<IServiceCollection> configureServices)
            : this(configureServices, EmptyAfterConfigureServices)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ActionTestConfiguration"/>
        /// </summary>
        /// <param name="afterConfigureServices">
        /// The action to execute after the services have been configured.
        /// </param>
        public ActionTestConfiguration(Action<IServiceProvider> afterConfigureServices)
            : this(EmptyConfigureServices, afterConfigureServices)
        {
        }

        /// <inheritdoc/>
        public void AfterConfigureServices(IServiceProvider provider) => _afterConfigureServices(provider);

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services) => _configureServices(services);
    }
}