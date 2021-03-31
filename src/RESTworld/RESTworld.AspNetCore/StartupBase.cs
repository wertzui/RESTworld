using AutoMapper;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Swagger;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableDependencyInjection();
            });
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
        }

        /// <summary>
        /// Migrates the specified database to the latest version.
        /// Call this inside your <see cref="Configure(IApplicationBuilder, IWebHostEnvironment)"/> method.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the database context.</typeparam>
        /// <param name="app">The application builder.</param>
        protected static void MigrateDatabase<TDbContext>(IApplicationBuilder app)
            where TDbContext : DbContext
        {
            var factory = app.ApplicationServices.GetRequiredService<IDbContextFactory<TDbContext>>();

            using var context = factory.CreateDbContext();
            context.Database.Migrate();
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