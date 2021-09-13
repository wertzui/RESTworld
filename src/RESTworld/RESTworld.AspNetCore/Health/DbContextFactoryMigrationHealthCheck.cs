using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health
{
    /// <summary>
    /// Checks if all migrations have been applied to the database.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DbContextFactoryMigrationHealthCheck<TContext> : IHealthCheck
        where TContext : DbContext
    {
        private readonly string _contextName;
        private readonly IDbContextFactory<TContext> _factory;

        /// <summary>
        /// Creates a new instance of the <see cref="DbContextFactoryMigrationHealthCheck{TContext}"/> class.
        /// </summary>
        /// <param name="factory">The factory which is used to create the <see cref="DbContext"/>.</param>
        public DbContextFactoryMigrationHealthCheck(IDbContextFactory<TContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
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
                    return new HealthCheckResult(context.Registration.FailureStatus, $"The following migrations are still pending for {_contextName}:{Environment.NewLine}{string.Join(Environment.NewLine, pendingMigrations)}");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, $"Cannot get the list of pending migrations for {_contextName}.", e);
            }

            return HealthCheckResult.Healthy($"No pending migrations for {_contextName}.");
        }
    }
}