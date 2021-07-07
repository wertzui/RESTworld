using AutoMapper;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Health;
using RESTworld.AspNetCore.Serialization;
using RESTworld.AspNetCore.Swagger;
using System.Reflection;
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
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", Assembly.GetEntryAssembly().GetName().Name);
                c.ConfigObject.DeepLinking = true;
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
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();
            services.AddSingleton(_ => ODataModelBuilder.GetEdmModel());

            services.Configure<RestWorldOptions>(Configuration.GetSection("RESTworld"));

            services
                .AddControllers(options =>
                {
                    options.OutputFormatters.RemoveType<ODataOutputFormatter>();
                    options.InputFormatters.RemoveType<ODataInputFormatter>();
                })
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.Add(new HomeControllerFeatureProvider());
                    manager.FeatureProviders.Add(new CrudControllerFeatureProvider());
                })
                .AddHALOData()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
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

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<SwaggerIgnoreOperationFilter>();
            });

            services.AddAutoMapper(ConfigureAutomapper);

            services.AddAuthentication();
            services.AddAuthorization();

            services.AddHttpContextAccessor();
            services.AddUserAccessor();

            services.AddHealthChecks();
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