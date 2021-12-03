using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Client.AspNetCore.Controllers;

namespace RESTworld.Client.AspNetCore
{
    /// <summary>
    /// A base startup class for an Angular application.
    /// It automatically configures everything needed to match the ASP.Net Core SPA template.
    /// The Angular app should be in the /ClientApp folder and the compiled app in the /ClientApp/dist folder.
    /// "npm start" should start the development server.
    /// </summary>
    public class StartupBase : RESTworld.AspNetCore.StartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupBase"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        public StartupBase(IConfiguration configuration)
            : base(configuration)
        {
        }

        /// <inheritdoc/>
        public override void ConfigureServices(IServiceCollection services)
        {
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.Configure<DependencyInjection.RestWorldOptions>(Configuration.GetSection(nameof(RestWorldOptions)));

            base.ConfigureServices(services);

            // The default Home Controller interferes with our SPA so we have to remove it.
            services
                .AddControllers()
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.Add(new RemoveHomeControllerFeatureProvider());
                    manager.FeatureProviders.Add(new SettingsControllerFeatureProvider());
                });
        }

        /// <inheritdoc/>
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {

            base.Configure(app, env, provider);
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
