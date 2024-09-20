using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health;

/// <summary>
/// Checks if all migrations have been applied to the database.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public partial class DbContextFactoryMigrationHealthCheck<TContext> : IHealthCheck
    where TContext : DbContext
{
    private readonly string _contextName;
    private readonly IDbContextFactory<TContext> _factory;
    private readonly ILogger<DbContextFactoryMigrationHealthCheck<TContext>> _logger;

    private const string _pendingMigrationsLogMessage = "The following migrations are still pending for {Context}:\n{PendingMigrations}";
    private static readonly CompositeFormat _pendingMigrationsMessageCompositeFormat = CompositeFormat.Parse(_pendingMigrationsLogMessage.Replace("{Context}", "{0}").Replace("{PendingMigrations}", "{1}"));

    private const string _exceptionMessageLogFormat = "Cannot get the list of pending migrations for {Context}.";
    private static readonly CompositeFormat _exceptionMessageCompositeFormat = CompositeFormat.Parse(_exceptionMessageLogFormat.Replace("{Context}", "{0}"));

    private const string _noPendingMigrationsMessage = "No pending migrations for {Context}.";
    private static readonly CompositeFormat _noPendingMigrationsMessageCompositeFormat = CompositeFormat.Parse(_noPendingMigrationsMessage.Replace("{Context}", "{0}"));

    /// <summary>
    /// Creates a new instance of the <see cref="DbContextFactoryMigrationHealthCheck{TContext}"/> class.
    /// </summary>
    /// <param name="factory">The factory which is used to create the <see cref="DbContext"/>.</param>
    /// <param name="logger">The logger.</param>
    public DbContextFactoryMigrationHealthCheck(IDbContextFactory<TContext> factory, ILogger<DbContextFactoryMigrationHealthCheck<TContext>> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contextName = typeof(TContext).Name;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = _factory.CreateDbContext();
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                LogPendingMigrations(_contextName, pendingMigrations);
                return new HealthCheckResult(context.Registration.FailureStatus, string.Format(CultureInfo.InvariantCulture, _pendingMigrationsMessageCompositeFormat, _contextName, string.Join(Environment.NewLine, pendingMigrations)));
            }
        }
        catch (Exception e)
        {
            LogExceptionMessage(_contextName, e);
            return new HealthCheckResult(context.Registration.FailureStatus, string.Format(CultureInfo.InvariantCulture, _exceptionMessageCompositeFormat, _contextName), e);
        }

        return HealthCheckResult.Healthy(string.Format(CultureInfo.InvariantCulture, _noPendingMigrationsMessageCompositeFormat, _contextName));
    }

    [LoggerMessage(LogLevel.Error, _pendingMigrationsLogMessage)]
    internal partial void LogPendingMigrations(string context, IEnumerable<string> pendingMigrations);

    [LoggerMessage(LogLevel.Error, _exceptionMessageLogFormat)]
    internal partial void LogExceptionMessage(string context, Exception exception);
}