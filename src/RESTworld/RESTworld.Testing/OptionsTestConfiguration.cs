using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace RESTworld.Testing
{
    /// <summary>
    /// Contains extension methods related to <see cref="OptionsTestConfiguration{TOptions}"/>.
    /// </summary>
    public static class OptionsTestConfigurationExtensions
    {
        /// <summary>
        /// Adds options to your tests.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="options">The options instance to add.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithOptions<TOptions>(this ITestBuilder builder, TOptions options)
            where TOptions : class
        {
            return builder.With(new OptionsTestConfiguration<TOptions>(options));
        }

        /// <summary>
        /// Adds options to your tests.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithOptions<TOptions>(this ITestBuilder builder, Action<TOptions> configureOptions)
            where TOptions : class
        {
            return builder.With(new OptionsTestConfiguration<TOptions>(configureOptions));
        }
    }

    /// <summary>
    /// This test configuration can add <see cref="IOptions{TOptions}"/> to your tests.
    /// </summary>
    public class OptionsTestConfiguration<TOptions> : ITestConfiguration
            where TOptions : class
    {
        private readonly TOptions? _options;
        private readonly Action<TOptions>? configureOptions;

        /// <summary>
        /// Creates a new instance of the <see cref="OptionsTestConfiguration{TOptions}"/> class.
        /// </summary>
        /// <param name="options">The options instance to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/>.</exception>
        public OptionsTestConfiguration(TOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="OptionsTestConfiguration{TOptions}"/> class.
        /// </summary>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configureOptions"/>.</exception>
        public OptionsTestConfiguration(Action<TOptions> configureOptions)
        {
            this.configureOptions = configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));
        }

        /// <inheritdoc/>
        public void AfterConfigureServices(IServiceProvider provider)
        {
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            if (_options is not null)
                services.AddSingleton(Options.Create(_options));
            else if (configureOptions is not null)
                services.Configure(configureOptions);
        }
    }
}