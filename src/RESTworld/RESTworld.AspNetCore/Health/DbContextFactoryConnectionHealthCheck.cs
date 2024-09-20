using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health;

/// <summary>
/// Checks if a connection to the database can be established.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public partial class DbContextFactoryConnectionHealthCheck<TContext> : IHealthCheck
    where TContext : DbContext
{
    private readonly string _contextName;
    private readonly IDbContextFactory<TContext> _factory;
    private readonly ILogger<DbContextFactoryConnectionHealthCheck<TContext>> _logger;
    private const string _exceptionMessageLogFormat = "Cannot connect to {Context}.";
    private static readonly CompositeFormat _exceptionMessageCompositeFormat = CompositeFormat.Parse(_exceptionMessageLogFormat.Replace("{Context}", "{0}"));

    private const string _successMessageLogFormat = "Connection to {Context} was successful.";
    private static readonly CompositeFormat _successMessageCompositeFormat = CompositeFormat.Parse(_successMessageLogFormat.Replace("{Context}", "{0}"));

    /// <summary>
    /// Creates a new instance of the <see cref="DbContextFactoryConnectionHealthCheck{TContext}"/> class.
    /// </summary>
    /// <param name="factory">The factory which is used to create the <see cref="DbContext"/>.</param>
    /// <param name="logger">The logger.</param>
    public DbContextFactoryConnectionHealthCheck(IDbContextFactory<TContext> factory, ILogger<DbContextFactoryConnectionHealthCheck<TContext>> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contextName = typeof(TContext).Name;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _factory.CreateDbContext();

        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        if (!canConnect)
        {
            try
            {
                await dbContext.Database.OpenConnectionAsync(cancellationToken);
            }
            catch (Exception e)
            {
                LogExceptionMessage(_contextName, e);
                return new HealthCheckResult(context.Registration.FailureStatus, string.Format(CultureInfo.InvariantCulture, _exceptionMessageCompositeFormat, _contextName));
            }

            return new HealthCheckResult(context.Registration.FailureStatus, string.Format(CultureInfo.InvariantCulture, _exceptionMessageCompositeFormat, _contextName));
        }

        return HealthCheckResult.Healthy(string.Format(CultureInfo.InvariantCulture, _successMessageCompositeFormat, _contextName));
    }

    [LoggerMessage(LogLevel.Error, _exceptionMessageLogFormat)]
    internal partial void LogExceptionMessage(string context, Exception? exception);
}