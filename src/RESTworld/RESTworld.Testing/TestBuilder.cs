using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTworld.Testing;

/// <summary>
/// A builder that is used to create the test environment when testing RESTworld services.
/// </summary>
public class TestBuilder : ITestBuilder
{
    private readonly List<ITestConfiguration> _configurations = new(8);

    /// <inheritdoc/>
    public ITestEnvironment Build()
    {
        var provider = BuildProvider();

        var environment = new TestEnvironment(provider);

        return environment;
    }

    /// <inheritdoc/>
    public ITestEnvironment<TSut> Build<TSut>()
        where TSut : class
    {
        var provider = BuildProvider(typeof(TSut));

        var environment = new TestEnvironment<TSut>(provider);

        return environment;
    }

    /// <inheritdoc/>
    public ITestBuilderWithConfig<TConfig> With<TConfig>(TConfig configuration)
        where TConfig : ITestConfiguration
    {
        _configurations.Add(configuration);
        return new TestBuilderWithConfig<TConfig>(this, configuration);
    }

    private ServiceProvider BuildProvider(Type? sutType = null)
    {
        var services = new ServiceCollection();
        foreach (var configuration in _configurations)
        {
            configuration.ConfigureServices(services);
        }

        // This way it will ensure that TSut is registered in the test environment if it has not been registered before.
        if (sutType is not null && !services.Any(s => s.ServiceType == sutType))
            services.AddSingleton(sutType);

        services.AddLogging(c => c.AddConsole());

        var provider = services.BuildServiceProvider();
        foreach (var configuration in _configurations)
        {
            configuration.AfterConfigureServices(provider);
        }

        return provider;
    }
}

/// <summary>
/// A builder that can also further configure the current <typeparamref name="TConfig"/>.
/// </summary>
/// <typeparam name="TConfig">The type of the current configuration.</typeparam>
public class TestBuilderWithConfig<TConfig> : ITestBuilderWithConfig<TConfig>
    where TConfig : ITestConfiguration
{
    private readonly ITestBuilder _testBuilder;

    /// <summary>
    /// Creates a new instance of the <see cref="TestBuilderWithConfig{TConfig}"/> class. This
    /// is normally done automatically when using the
    /// <see cref="ITestBuilder.With{TConfig}(TConfig)"/> method.
    /// </summary>
    /// <param name="testBuilder">The test builder that is currently being used.</param>
    /// <param name="config">The configuration that has just been added to the test builder.</param>
    /// <exception cref="ArgumentNullException"><paramref name="testBuilder"/>, <paramref name="config"/>.</exception>
    public TestBuilderWithConfig(ITestBuilder testBuilder, TConfig config)
    {
        _testBuilder = testBuilder ?? throw new ArgumentNullException(nameof(testBuilder));
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    public TConfig Config { get; }

    /// <inheritdoc/>

    public ITestEnvironment Build()
    {
        return _testBuilder.Build();
    }

    /// <inheritdoc/>

    public ITestEnvironment<TSut> Build<TSut>()
        where TSut : class
    {
        return _testBuilder.Build<TSut>();
    }

    /// <inheritdoc/>

    public ITestBuilderWithConfig<TNewConfig> With<TNewConfig>(TNewConfig configuration)
        where TNewConfig : ITestConfiguration
    {
        return _testBuilder.With(configuration);
    }
}