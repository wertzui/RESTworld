using Microsoft.Extensions.DependencyInjection;

namespace RESTworld.Testing;

/// <summary>
/// A test environment is used to retrieve instances of the classes that are used in the test
/// and have been configured with an <see cref="ITestBuilder"/>.
/// </summary>
/// <typeparam name="TSut">The type of the system under test.</typeparam>
public class TestEnvironment<TSut> : TestEnvironment, ITestEnvironment<TSut>
    where TSut : class
{
    /// <summary>
    /// Creates a new instance of the <see cref="TestEnvironment{TSut}"/> class. This is
    /// normally done by calling the <see cref="ITestBuilder.Build{TSut}"/> method.
    /// </summary>
    /// <param name="provider">The provider to use when requesting service instances.</param>
    public TestEnvironment(ServiceProvider provider)
        : base(provider)
    {
    }

    /// <inheritdoc/>
    public TSut GetSut() => GetRequiredService<TSut>();
}

/// <summary>
/// A test environment is used to retrieve instances of the classes that are used in the test
/// and have been configured with an <see cref="ITestBuilder"/>.
/// </summary>
public class TestEnvironment : ITestEnvironment
{
    private readonly ServiceProvider _provider;

    /// <summary>
    /// Creates a new instance of the <see cref="TestEnvironment"/> class. This is
    /// normally done by calling the <see cref="ITestBuilder.Build"/> method.
    /// </summary>
    /// <param name="provider">The provider to use when requesting service instances.</param>
    public TestEnvironment(ServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc/>
    public void Dispose() => _provider.Dispose();

    /// <inheritdoc/>
    public T GetRequiredService<T>() where T : notnull => _provider.GetRequiredService<T>();

    /// <inheritdoc/>
    public T? GetService<T>() => _provider.GetService<T>();
}