﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace RESTworld.AspNetCore;

/// <summary>
/// The base class for your program.
/// It will automatically add logging and configure your web host using the <typeparamref name="TStartup"/> class.
/// </summary>
/// <typeparam name="TStartup">Your implementation of a startup class. Normally this will override <see cref="StartupBase"/>.</typeparam>
public static class Program<TStartup>
    where TStartup : class
{
    /// <summary>
    /// The main entry point for the program.
    /// </summary>
    /// <param name="args">Command line arguments which are added to the configuration.</param>
    /// <param name="configureHostBuilder">An optional function to further configure the host builder.</param>
    public static void Main(string[] args, Func<IHostBuilder, IHostBuilder>? configureHostBuilder = null)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateBootstrapLogger();
        try
        {
            Log.Information("Starting up");
            var hostBuilder = CreateHostBuilder(args);

            if (configureHostBuilder is not null)
                hostBuilder = configureHostBuilder(hostBuilder);

            hostBuilder.Build().Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) =>
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext())
            .ConfigureAppConfiguration(b => b.AddUserSecrets<TStartup>())
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TStartup>());
}