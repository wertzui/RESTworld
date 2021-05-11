using AutoMapper;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Health;
using RESTworld.AspNetCore.HostedServices;
using RESTworld.AspNetCore.Swagger;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore
{
    public abstract class StartupBase
    {
        public StartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        internal static ODataConventionModelBuilder ODataModelBuilder { get; } = new ODataConventionModelBuilder();

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

        // This method gets called by the runtime. Use this method to add services to the container.
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