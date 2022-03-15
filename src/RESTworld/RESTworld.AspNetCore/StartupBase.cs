using AutoMapper;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Formatter;
using RESTworld.AspNetCore.Health;
using RESTworld.AspNetCore.Serialization;
using RESTworld.AspNetCore.Swagger;
using RESTworld.AspNetCore.Validation;
using RESTworld.AspNetCore.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore
{
    /// <summary>
    /// A base class for your Startup implementation.
    /// It will automatically add everything which is needed for a REST pipeline to work.
    /// </summary>
    public abstract class StartupBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="StartupBase"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
        public StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The <see cref="IConfiguration"/> instance.
        /// </summary>
        public IConfiguration Configuration { get; }

        internal static ODataConventionModelBuilder ODataModelBuilder { get; } = new ODataConventionModelBuilder();

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="env">The <see cref="IWebHostBuilder"/> instance.</param>
        /// <param name="provider"></param>
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

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
                foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                options.ConfigObject.DeepLinking = true;

                // When sending large files in Swagger UI, these should be downloaded instead of displayed.
                options.UseResponseInterceptor(@"
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
}"
.Replace("\r", "").Replace("\n", ""));
            });

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableDependencyInjection();
            });

            app.UseHealthChecks("/health/startup", new HealthCheckOptions { Predicate = r => r.Tags.Contains("startup"), ResponseWriter = HealthCheckHALResponseWriter.WriteResponseAsync });
            app.UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live"), ResponseWriter = HealthCheckHALResponseWriter.WriteResponseAsync });
            app.UseHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready"), ResponseWriter = HealthCheckHALResponseWriter.WriteResponseAsync });

            app.UseResponseCompression();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // Default version
                var defaultVersion = Configuration.GetValue("RESTworld:Versioning:DefaultVersion", "*");
                if (defaultVersion == "*")
                {
                    options.ApiVersionSelector = new LatestApiVersionSelector();
                }
                else
                {
                    if (!ApiVersion.TryParse(defaultVersion, out var parsedVersion) || parsedVersion is null)
                        throw new ArgumentOutOfRangeException("RESTworld:Versioning:DefaultVersion", defaultVersion, "The setting for \"RESTworld:Versioning:DefaultVersion\" was neither \"*\" nor a valid API version.");

                    options.DefaultApiVersion = parsedVersion;
                }

                // Version parameter
                var parameterName = Configuration.GetValue("RESTworld:Versioning:ParameterName", "v");
                var allowQueryStringVersioning = Configuration.GetValue("RESTworld:Versioning:AllowQueryParameterVersioning", false);
                if (allowQueryStringVersioning)
                {
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new MediaTypeApiVersionReader(parameterName),
                        new QueryStringApiVersionReader(parameterName));
                }
                else
                {
                    options.ApiVersionReader = new MediaTypeApiVersionReader(parameterName);
                }
            });

            services.AddOData();
            services.AddSingleton(_ => ODataModelBuilder.GetEdmModel());

            services.Configure<RestWorldOptions>(Configuration.GetSection("RESTworld"));

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
                .AddHALOData()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase));
                    o.JsonSerializerOptions.Converters.Add(new SingleObjectOrCollectionJsonConverterFactory());
                });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(_ => true) // allow any origin
                        .AllowCredentials()); // allow credentials
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";

                // Do not advertise versioning through query parameter as this is only intended for legacy clients and should not be visible as it is not considered RESTfull.
                if (options.ApiVersionParameterSource is not MediaTypeApiVersionReader)
                {
                    var parameterName = Configuration.GetValue("RESTworld:Versioning:ParameterName", "v");
                    options.ApiVersionParameterSource = new MediaTypeApiVersionReader(parameterName);
                }
            });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureVersioningWithSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                // Remove the ODataQueryOptions which is used on the GetList endpoint from the schema and the operation.
                options.OperationFilter<SwaggerIgnoreOperationFilter>();
                options.MapType(typeof(ODataQueryOptions<>), () => new());

                // Add versioning through media type.
                var parameterName = Configuration.GetValue("RESTworld:Versioning:ParameterName", "v");
                options.OperationFilter<SwaggerVersioningOperationFilter>(parameterName);

                // Add meaningfull examples.
                options.OperationFilter<SwaggerExampleOperationFilter>();
            });

            services.AddSingleton<ProblemDetailsFactory, RestWorldProblemDetailsFactory>();

            services.AddAutoMapper(ConfigureAutomapper);

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
                options.MimeTypes = new[]
                {
                    "text/*",
                    "application/*",
                    "font/*",
                    "image/svg+xml"
                };
            });
        }

        /// <summary>
        /// Override this method and add your AutoMapper configuration logic.
        /// </summary>
        /// <param name="config">The configuration.</param>
        protected virtual void ConfigureAutomapper(IMapperConfigurationExpression config)
        {
        }
    }
}