using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTworld.AspNetCore.Controller;
using RESTworld.Client.AspNetCore.Controllers;
using System;

namespace Microsoft.AspNetCore.Builder;
/// <summary>
/// Contains extension methods to add the RESTworld client to an ASP.Net Core application with an Angular frontend.
/// It automatically configures everything needed to match the ASP.Net Core SPA template.
/// The Angular app should be in the /ClientApp folder and the compiled app in the /ClientApp/dist folder.
/// "npm start" should start the development server.
/// </summary>
public static class RestWorldSpaClientBuilderExtensions
{
    /// <summary>
    /// Adds everything needed to use RESTworld with an Angular frontend.
    /// Don't forget to call <see cref="UseRestWorldWithSpaFrontend(WebApplication, string)"/> afterwards.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="contentRoot">The path from where the compiled Angular application is served in the published application in a production environment.</param>
    public static RestWorldWebApplicationBuilder AddRestWorldWithSpaFrontend(this WebApplicationBuilder builder, string contentRoot = "ClientApp/dist")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);

        var services = builder.Services;

        // In production, the Angular files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = contentRoot;
        });

        var rwBuilder = builder.AddRestWorld();

        // The default Home Controller interferes with our SPA so we have to remove it.
        services
            .AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new RemoveHomeControllerFeatureProvider());
                manager.FeatureProviders.Add(new SettingsControllerFeatureProvider());
            });

        return rwBuilder;
    }

    /// <summary>
    /// Adds everything needed to use RESTworld with an Angular frontend.
    /// Don't forget to call <see cref="AddRestWorldWithSpaFrontend(WebApplicationBuilder, string)"/> before this.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="sourcePath">The path of the directory that contains the SPA source files during development. The directory may not exist in published applications.</param>
    public static WebApplication UseRestWorldWithSpaFrontend(this WebApplication app, string sourcePath = "ClientApp")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        app.UseRestWorld();

        app.UseStaticFiles();
        if (!app.Environment.IsDevelopment())
        {
            app.UseSpaStaticFiles();
        }

        app.UseSpa(spa =>
        {
            // To learn more about options for serving an Angular SPA from ASP.NET Core,
            // see https://go.microsoft.com/fwlink/?linkid=864501

            spa.Options.SourcePath = sourcePath;

            if (app.Environment.IsDevelopment())
            {
                spa.UseAngularCliServer(npmScript: "start");
            }
        });

        return app;
    }
}
