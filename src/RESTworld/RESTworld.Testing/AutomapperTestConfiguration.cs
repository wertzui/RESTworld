using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RESTworld.Testing;

/// <summary>
/// Contains extension methods related to <see cref="AutomapperTestConfiguration"/>.
/// </summary>
public static class AutomapperTestConfigurationExtensions
{
    /// <summary>
    /// Adds Auto Mapper to the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The builder to add the configuration to.</param>
    /// <param name="configureAutomapper">An action to configure Auto Mapper.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static ITestBuilder WithAutomapper(this ITestBuilder builder, Action<IMapperConfigurationExpression> configureAutomapper)
    {
        return builder.With(new AutomapperTestConfiguration(configureAutomapper));
    }
}

/// <summary>
/// This test configuration can add Auto Mapper to your tests.
/// </summary>
public class AutomapperTestConfiguration : ITestConfiguration
{
    private readonly Mapper _mapper;

    /// <summary>
    /// Creates a new instance of the <see cref="AutomapperTestConfiguration"/> class.
    /// </summary>
    /// <param name="configureAutomapper">An action to configure Auto Mapper.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configureAutomapper"/></exception>
    public AutomapperTestConfiguration(Action<IMapperConfigurationExpression> configureAutomapper)
    {
        ArgumentNullException.ThrowIfNull(configureAutomapper);

        _mapper = new Mapper(new MapperConfiguration(configureAutomapper));
    }

    /// <inheritdoc/>
    public void AfterConfigureServices(IServiceProvider provider)
    {
    }

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMapper>(_mapper);
    }
}