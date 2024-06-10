using Asp.Versioning;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.ContentNegotiation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.ModelBuilder;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.Formatter;
using RESTworld.AspNetCore.Health;
using RESTworld.AspNetCore.Links;
using RESTworld.AspNetCore.Links.Abstractions;
using RESTworld.AspNetCore.Results;
using RESTworld.AspNetCore.Results.Abstractions;
using RESTworld.AspNetCore.Serialization;
using RESTworld.AspNetCore.Swagger;
using RESTworld.AspNetCore.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Contains extension methods for the <see cref="WebApplicationBuilder"/> class.
/// It will automatically add everything which is needed for a REST pipeline to work.
/// </summary>
public static class RestWorldBuilderExtensions
{
    /// <summary>
    /// Adds RESTworld to the application.
    /// It will automatically add everything which is needed for a REST pipeline to work.
    /// Don't forget to call <see cref="UseRestWorld(WebApplication)"/> afterwards.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>A <see cref="RestWorldWebApplicationBuilder"/> which wraps the given <see cref="WebApplicationBuilder"/> and additionally exposes <see cref="RestWorldWebApplicationBuilder.ODataModelBuilder"/> to configure OData.</returns>
    /// <exception cref="ArgumentNullException">The setting for "RESTworld: Versioning:ParameterName" must not be null. If you want the default value ("v"), just leave it out.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The setting for "RESTworld:Versioning:DefaultVersion" was neither "*" nor a valid API version.</exception>
    public static RestWorldWebApplicationBuilder AddRestWorld(this WebApplicationBuilder builder)
    {
        var restWorldBuilder = new RestWorldWebApplicationBuilder(builder);
        var configuration = builder.Configuration;
        var services = builder.Services;

        restWorldBuilder.AddRestWorldOptions();

        var versionParameterName = configuration.GetValue("RESTworld:Versioning:ParameterName", "v");
        if (versionParameterName is null)
            throw new ArgumentNullException("RESTworld:Versioning:ParameterName", """The setting for "RESTworld: Versioning:ParameterName" must not be null. If you want the default value ("v"), just leave it out.""");

        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // Default version
                var defaultVersion = configuration.GetValue("RESTworld:Versioning:DefaultVersion", "*");
                if (defaultVersion == "*")
                {
                    options.ApiVersionSelector = new LatestApiVersionSelector();
                }
                else
                {
                    if (!ApiVersionParser.Default.TryParse(defaultVersion, out var parsedVersion) || parsedVersion is null)
                        throw new ArgumentOutOfRangeException("RESTworld:Versioning:DefaultVersion", defaultVersion, """The setting for "RESTworld:Versioning:DefaultVersion" was neither "*" nor a valid API version.""");

                    options.DefaultApiVersion = parsedVersion;
                }

                // Version parameter
                // Newer Chromium based browsers are always sending "application/signed-exchange;v=b3" in the accept header which will otherwise be interpreted as an invalid version.
                options.ApiVersionReader = new MediaTypeApiVersionReaderBuilder().Parameter(versionParameterName).Exclude("application/signed-exchange").Build();

                var allowQueryStringVersioning = configuration.GetValue("RESTworld:Versioning:AllowQueryParameterVersioning", false);
                if (allowQueryStringVersioning)
                    options.ApiVersionReader = ApiVersionReader.Combine(options.ApiVersionReader, new QueryStringApiVersionReader(versionParameterName));
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";

                // Do not advertise versioning through query parameter as this is only intended for legacy clients and should not be visible as it is not considered RESTfull.
                if (options.ApiVersionParameterSource is not MediaTypeApiVersionReader)
                {
                    options.ApiVersionParameterSource = new MediaTypeApiVersionReader(versionParameterName);
                }
            })
            .AddMvc();

        services.AddSingleton(_ => restWorldBuilder.ODataModelBuilder.GetEdmModel());

        services.AddSingleton<ICacheHelper, CacheHelper>();
        services.AddSingleton<IResultFactory, ResultFactory>();
        services.AddSingleton<ICrudLinkFactory, CrudLinkFactory>();
        services.AddSingleton<ILinkFactory, CrudLinkFactory>();

        services
            .AddControllers(options =>
            {
                options.OutputFormatters.RemoveType<ODataOutputFormatter>();
                options.InputFormatters.RemoveType<ODataInputFormatter>();
                options.OutputFormatters.Add(new CsvOutputFormatter());
                options.RespectBrowserAcceptHeader = true;
            })
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new HomeControllerFeatureProvider());
                manager.FeatureProviders.Add(new RestControllerFeatureProvider());
            })
            .AddOData()
            .AddHALOData()
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.Converters.Add(new SingleObjectOrCollectionJsonConverterFactory());
            });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(HeaderNames.Location, "Api-Deprecated-Versions", "Api-Supported-Versions")
                    .SetIsOriginAllowed(_ => true) // allow any origin
                    .AllowCredentials()); // allow credentials
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureVersioningWithSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            // Remove the ODataQueryOptions which is used on the GetList endpoint from the schema and the operation.
            options.OperationFilter<SwaggerODataOperationFilter>();
            options.OperationFilter<SwaggerIgnoreOperationFilter>();
            options.MapType(typeof(ODataQueryOptions<>), () => new());

            // Add versioning through media type.
            options.OperationFilter<SwaggerVersioningOperationFilter>(versionParameterName);

            // Add meaningful examples.
            options.OperationFilter<SwaggerExampleOperationFilter>();
        });

        services.AddValidationAndErrorHandling();

        services.AddAuthentication();
        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddUserAccessor();

        services.AddHealthChecks();

        services.AddResponseCompression(options =>
        {
            // The server does not send secure cookies and cannot be used to send arbitrary payloads by an attacker.
            // The Bearer Token is only present in requests and not in responses.
            // Therefore it is not vulnerable to CRIME and BREACH.
            options.EnableForHttps = true;
            options.MimeTypes =
            [
                "text/*",
                "application/*",
                "font/*",
                "image/svg+xml"
            ];
        });

        builder.ConfigureOpenTelemetry();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return restWorldBuilder;
    }

    private static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder,
        Action<OpenTelemetryLoggerOptions>? configureLogging = null,
        Action<MeterProviderBuilder>? configureMetrics = null,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        builder.Logging.AddOpenTelemetry(configureLogging ?? (logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        }));

        builder.Services.AddOpenTelemetry()
            .WithMetrics(configureMetrics ?? (metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            }))
            .WithTracing((tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation(opts =>
                    {
                        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                        opts.RecordException = true;
                        opts.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            AddHttpHeadersToActivity(activity, request.Headers, jsonOptions);
                        };
                        opts.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            AddHttpHeadersToActivity(activity, response.Headers, jsonOptions);
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(opts =>
                    {
                        opts.SetDbStatementForStoredProcedure = true;
                        opts.SetDbStatementForText = true;
                        opts.EnrichWithIDbCommand = (activity, command) =>
                        {
                            MoveDbStatementTagToDbQueryText(activity);
                            AddDbQueryParameterTagsToActivity(activity, command);
                        };
                    });
                // When using both Instrumentations, there is a bug, causing spans to not use the correct parent.
                // See https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/1764
                //.AddSqlClientInstrumentation(opt =>
                //{
                //    opt.EnableConnectionLevelAttributes = true;
                //    opt.RecordException = true;
                //    opt.SetDbStatementForStoredProcedure = true;
                //    opt.SetDbStatementForText = true;
                //    opt.Enrich = (activity, eventName, command) =>
                //    {
                //        MoveDbStatementTagToDbQueryText(activity);

                //        if (command is IDbCommand dbCommand)
                //            AddDbQueryParameterTagsToActivity(activity, dbCommand);
                //    };
                //});
            }));

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static void AddDbQueryParameterTagsToActivity(Activity activity, IDbCommand command)
    {
        foreach (IDataParameter parameter in command.Parameters)
        {
            var key = "db.query.parameter." + parameter.ParameterName;
            activity.SetTag(key, parameter.Value);
        }
    }

    private static void MoveDbStatementTagToDbQueryText(Activity activity)
    {
        var statement = activity.GetTagItem("db.statement");
        var queryText = activity.GetTagItem("db.query.text");
        if (statement is not null)
        {
            activity.SetTag("db.statement", null);
            if (queryText is null)
                activity.SetTag("db.query.text", statement);
        }
    }

    private static void AddHttpHeadersToActivity(Activity activity, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, JsonSerializerOptions jsonOptions)
    {
        foreach (var header in headers)
        {
            var key = header.Key.ToLowerInvariant();
            var value = JsonSerializer.Serialize(header.Value, jsonOptions);
            activity.SetTag($"http.request.header.{key}", value);

            if (key == "content-length")
                activity.SetTag("http.request.body.size", value);
        }
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    /// <summary>
    /// Adds an ODataModel for a <see cref="DbContext"/>. This is required for List operations
    /// to work.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The builder to add the model to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static RestWorldWebApplicationBuilder AddODataModelForDbContext<TContext>(this RestWorldWebApplicationBuilder builder)
        where TContext : DbContext
    {
        var dbSetType = typeof(DbSet<>);
        var entityTypes = typeof(TContext).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.PropertyType)
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == dbSetType)
            .Select(t => t.GenericTypeArguments[0]);

        foreach (var entityType in entityTypes)
        {
            builder.ODataModelBuilder.AddEntitySet(entityType.Name, new EntityTypeConfiguration(builder.ODataModelBuilder, entityType));
        }

        return builder;
    }

    /// <summary>
    /// Configures the application to use RESTworld and all needed middle ware.
    /// It will automatically add everything which is needed for a REST pipeline to work.
    /// Don't forget to call <see cref="AddRestWorld(WebApplicationBuilder)"/> before.
    /// </summary>
    public static WebApplication UseRestWorld(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseAcceptHeaders();

        app.UseRequestLocalization(options =>
        {
            // All cultures are supported as these will just be handed down to the appropriate serializer.
            options.DefaultRequestCulture = new(CultureInfo.InvariantCulture);

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            options.SupportedCultures = allCultures;
            options.SupportedCultures = allCultures;
        });

        app.UseRouting();

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            var apiVersionDescriptions = app.DescribeApiVersions();
            foreach (var description in apiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
            options.ConfigObject.DeepLinking = true;

            // When sending large files in Swagger UI, these should be downloaded instead of displayed.
            options.UseResponseInterceptor("""
                (res) => {
                    window.res = res;
                    if (res && res.status >= 200 && res.status < 300 && res.data && res.headers['content-type'] && res.url && !res.url.endsWith('swagger.json') && res.data.length > 1000000) {
                        try {
                            var blob = new Blob([res.data], { type: res.headers['content-type'] });
                            var blobURL = URL.createObjectURL(blob);
                            var tempLink = document.createElement('a');
                            tempLink.style.display = 'none';
                            tempLink.href = blobURL;
                            tempLink.setAttribute('download', new URL(res.url).pathname.replace('/', ''));
                            tempLink.click();
                            return { ok: res.ok, url: res.url, status: res.status, statusText: res.statusText, headers: res.headers, duration: res.duration };
                        }
                        catch { }
                    }
                    return res;
                }
                """
                .Replace("\r", "").Replace("\n", ""));
        });

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        // Although UseEndpoints is already called before by builder.Build(), we need to call it here again,
        // because otherwise the SPA Proxy will try to deliver the /Settings route instead of it being routed to the SettingsController.
        app.UseEndpoints(_ => { });
        app.MapControllers();

        app.UseHealthChecks("/health/startup", new HealthCheckOptions { Predicate = r => r.Tags.Contains("startup"), ResponseWriter = HealthCheckHALResponseWriter.WriteResponseAsync });
        app.UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live"), ResponseWriter = HealthCheckHALResponseWriter.WriteResponseAsync });
        app.UseHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready"), ResponseWriter = HealthCheckHALResponseWriter.WriteResponseAsync });

        app.UseResponseCompression();

        return app;
    }
}
