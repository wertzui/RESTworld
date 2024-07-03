using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;

namespace RESTworld.Testing;

/// <summary>
/// Contains extension methods related to <see cref="SubstituteTestConfiguration{TSubstitute}"/>.
/// </summary>
public static class SubstituteTestConfigurationExtensions
{
    /// <summary>
    /// Adds Auto Mapper to the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder to add the substitute to.</param>
    /// <param name="configureSubstitute">An action to configure Auto Mapper.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static ITestBuilderWithConfig<SubstituteTestConfiguration<TSubstitute>> WithSubstitute<TSubstitute>(this ITestBuilder builder, Action<TSubstitute>? configureSubstitute = null)
        where TSubstitute : class
    {
        return builder.With(new SubstituteTestConfiguration<TSubstitute>(configureSubstitute));
    }

    /// <summary>
    /// Further configures the substitute.
    /// </summary>
    /// <param name="builder">The builder with the substitute to add the configuration to.</param>
    /// <param name="configureSubstitute">An action to configure the substitute.</param>
    /// <returns>This instance.</returns>
    public static ITestBuilderWithConfig<SubstituteTestConfiguration<TSubstitute>> Configure<TSubstitute>(this ITestBuilderWithConfig<SubstituteTestConfiguration<TSubstitute>> builder, Action<TSubstitute> configureSubstitute)
        where TSubstitute : class
    {
        ArgumentNullException.ThrowIfNull(configureSubstitute);

        builder.Config.Configure(configureSubstitute);
        return builder;
    }
}

/// <summary>
/// This test configuration can add any substitute to your tests.
/// </summary>
public class SubstituteTestConfiguration<TSubstitute> : ITestConfiguration
    where TSubstitute : class
{
    private readonly TSubstitute _substitute;

    /// <summary>
    /// Creates a new instance of the <see cref="SubstituteTestConfiguration{TSubstitute}"/> class.
    /// </summary>
    /// <param name="configureSubstitute">An action to configure the substitute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configureSubstitute"/></exception>
    public SubstituteTestConfiguration(Action<TSubstitute>? configureSubstitute)
    {
        var substitute = Substitute.For<TSubstitute>();
        configureSubstitute?.Invoke(substitute);

        _substitute = substitute;
    }

    /// <summary>
    /// Further configures the substitute.
    /// </summary>
    /// <param name="configureSubstitute">An action to configure the substitute.</param>
    /// <returns>This instance.</returns>
    public SubstituteTestConfiguration<TSubstitute> Configure(Action<TSubstitute> configureSubstitute)
    {
        configureSubstitute(_substitute);
        return this;
    }

    /// <inheritdoc/>
    public void AfterConfigureServices(IServiceProvider provider)
    {
    }

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_substitute);
    }
}