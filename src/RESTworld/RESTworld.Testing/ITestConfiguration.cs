using Microsoft.Extensions.DependencyInjection;
using System;

namespace RESTworld.Testing
{
    /// <summary>
    /// A test configuration which can add services and further configure them.
    /// </summary>
    public interface ITestConfiguration
    {
        /// <summary>
        /// After services have been configured, instances of them can be retrieved here and
        /// configured further.
        /// </summary>
        /// <param name="provider">The service provider from which instances can be retrieved.</param>
        void AfterConfigureServices(IServiceProvider provider);

        /// <summary>
        /// Add services to the test environment.
        /// </summary>
        /// <param name="services">The service provider to which services are added to.</param>
        void ConfigureServices(IServiceCollection services);
    }
}