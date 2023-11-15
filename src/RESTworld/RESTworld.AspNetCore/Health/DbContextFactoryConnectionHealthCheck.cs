using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health;

/// <summary>
/// Checks if a connection to the database can be established.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public class DbContextFactoryConnectionHealthCheck<TContext> : IHealthCheck
    where TContext : DbContext
{
    private readonly string _contextName;
    private readonly IDbContextFactory<TContext> _factory;

    /// <summary>
    /// Creates a new instance of the <see cref="DbContextFactoryConnectionHealthCheck{TContext}"/> class.
    /// </summary>
    /// <param name="factory">The factory which is used to create the <see cref="DbContext"/>.</param>
    public DbContextFactoryConnectionHealthCheck(IDbContextFactory<TContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
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
                return new HealthCheckResult(context.Registration.FailureStatus, $"Cannot connect to {_contextName}", e);
            }

            return new HealthCheckResult(context.Registration.FailureStatus, $"Cannot connect to {_contextName}.");
        }

        return HealthCheckResult.Healthy($"Connection to {_contextName} was successfull.");
    }
}