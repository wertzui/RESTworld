using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.HostedServices;

/// <summary>
/// This hosted service will migrate databases asynchronously during startup.
/// This way the application can already respond to requests which do not need a database while the migrations are still being applied.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public class DatabaseMigrationHostedService<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private const int MIGRATION_TIMEOUT = int.MaxValue;
    private readonly IDbContextFactory<TDbContext> _factory;
    private readonly ILogger<DatabaseMigrationHostedService<TDbContext>> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="DatabaseMigrationHostedService{TDbContext}"/> class.
    /// </summary>
    /// <param name="factory">The <see cref="IDbContextFactory{TContext}"/> which is used to create the database context.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    public DatabaseMigrationHostedService(
        IDbContextFactory<TDbContext> factory,
        ILogger<DatabaseMigrationHostedService<TDbContext>> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var contextName = typeof(TDbContext).Name;
        try
        {
            _logger.LogInformation($"Starting migration of {contextName}.");
            await using var context = _factory.CreateDbContext();
            int? timeout = default;

            try
            {
                timeout = context.Database.GetCommandTimeout();
                context.Database.SetCommandTimeout(MIGRATION_TIMEOUT);

                await context.Database.MigrateAsync(stoppingToken);
            }
            finally
            {
                if (timeout != MIGRATION_TIMEOUT)
                    context.Database.SetCommandTimeout(timeout);
            }
            _logger.LogInformation($"Finished migration of {contextName}.");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"An exception occured during the migration of {contextName}.");
        }
    }
}