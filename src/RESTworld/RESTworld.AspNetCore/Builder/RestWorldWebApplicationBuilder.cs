using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Builder;


/// <summary>
/// A builder for creating a <see cref="WebApplication"/> with RESTworld services.
/// It exposes all the properties of a <see cref="WebApplicationBuilder"/> and adds additional properties for RESTworld.
/// </summary>
public class RestWorldWebApplicationBuilder : IHostApplicationBuilder
{
    private readonly WebApplicationBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestWorldWebApplicationBuilder"/> class.
    /// </summary>
    /// <param name="builder">The builder to wrap.</param>
    public RestWorldWebApplicationBuilder(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// The OData model builder which can be used to configure the OData model.
    /// Use builder.AddODataModelForDbContext() to automatically add all entities from a DbContext.
    /// </summary>
    public ODataConventionModelBuilder ODataModelBuilder { get; } = new ODataConventionModelBuilder();


    /// <summary>
    /// Provides information about the web hosting environment an application is running.
    /// </summary>
    public IHostEnvironment Environment => _builder.Environment;

    /// <summary>
    /// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
    /// </summary>
    public IServiceCollection Services => _builder.Services;

    /// <summary>
    /// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
    /// </summary>
    public IConfigurationManager Configuration => _builder.Configuration;

    /// <summary>
    /// A collection of logging providers for the application to compose. This is useful for adding new logging providers.
    /// </summary>
    public ILoggingBuilder Logging => _builder.Logging;

    /// <summary>
    /// Allows enabling metrics and directing their output.
    /// </summary>
    public IMetricsBuilder Metrics => _builder.Metrics;

    /// <summary>
    /// An <see cref="IWebHostBuilder"/> for configuring server specific properties, but not building.
    /// To build after configuration, call <see cref="Build"/>.
    /// </summary>
    public ConfigureWebHostBuilder WebHost => _builder.WebHost;

    /// <summary>
    /// An <see cref="IHostBuilder"/> for configuring host specific properties, but not building.
    /// To build after configuration, call <see cref="Build"/>.
    /// </summary>
    public ConfigureHostBuilder Host => _builder.Host;

    /// <inheritdoc/>
    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)_builder).Properties;

    /// <summary>
    /// Builds the <see cref="WebApplication"/>.
    /// </summary>
    /// <returns>A configured <see cref="WebApplication"/>.</returns>
    public WebApplication Build() => _builder.Build();

    /// <inheritdoc/>
    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull => ((IHostApplicationBuilder)_builder).ConfigureContainer(factory, configure);
}
