namespace RESTworld.Testing;

/// <summary>
/// A builder that is used to create the test environment when testing RESTworld services.
/// </summary>
public interface ITestBuilder
{
    /// <summary>
    /// Creates the test environment.
    /// </summary>
    /// <typeparam name="TSut">
    /// The type of the system under test. This is probably the type of your service.
    /// </typeparam>
    /// <returns>
    /// A test environment which can be used to retrieve the system under test and any other
    /// configured instances.
    /// </returns>
    ITestEnvironment<TSut> Build<TSut>() where TSut : class;

    /// <summary>
    /// Creates the test environment.
    /// </summary>
    /// <returns>
    /// A test environment which can be used to retrieve the system under test and any other
    /// configured instances.
    /// </returns>
    ITestEnvironment Build();

    /// <summary>
    /// Adds the given <paramref name="configuration"/> to the test environment.
    /// </summary>
    /// <typeparam name="TConfig">The type of the configuration.</typeparam>
    /// <param name="configuration">
    /// The configuration to add. If you are not creating new extension methods, but using this
    /// directly in your test, you probably want to insert an instance of <see cref="ActionTestConfiguration"/>.
    /// </param>
    /// <returns>
    /// The same test builder with the possibility to further configure the current
    /// configuration instance.
    /// </returns>
    ITestBuilderWithConfig<TConfig> With<TConfig>(TConfig configuration) where TConfig : ITestConfiguration;
}

/// <summary>
/// A builder that can also further configure the current <typeparamref name="TConfig"/>.
/// </summary>
/// <typeparam name="TConfig">The type of the current configuration.</typeparam>
public interface ITestBuilderWithConfig<TConfig> : ITestBuilder
    where TConfig : ITestConfiguration
{
    /// <summary>
    /// The last configuration that has been added to the builder.
    /// </summary>
    TConfig Config { get; }
}