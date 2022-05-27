using System;

namespace RESTworld.Testing
{
    /// <summary>
    /// A test environment is used to retrieve instances of the classes that are used in the test
    /// and have been configured with an <see cref="ITestBuilder"/>.
    /// </summary>
    /// <typeparam name="TSut">The type of the system under test.</typeparam>
    public interface ITestEnvironment<TSut> : ITestEnvironment
        where TSut : class
    {
        /// <summary>
        /// Get the system under test from the test environment.
        /// </summary>
        /// <returns>A object of type <typeparamref name="TSut"/>.</returns>
        TSut GetSut();
    }

    /// <summary>
    /// A test environment is used to retrieve instances of the classes that are used in the test
    /// and have been configured with an <see cref="ITestBuilder"/>.
    /// </summary>
    public interface ITestEnvironment : IDisposable
    {
        /// <summary>
        /// Get service of type T from the test environment.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
        /// <returns>A service object of type <typeparamref name="T"/>.</returns>
        T GetRequiredService<T>() where T : notnull;

        /// <summary>
        /// Get service of type T from the test environment.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <returns>
        /// A service object of type <typeparamref name="T"/> or null if there is no such service.
        /// </returns>
        T? GetService<T>();
    }
}