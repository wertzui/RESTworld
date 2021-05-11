using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health
{
    public class DbContextFactoryMigrationHealthCheck<TContext> : IHealthCheck
        where TContext : DbContext
    {
        private readonly string _contextName;
        private readonly IDbContextFactory<TContext> _factory;

        public DbContextFactoryMigrationHealthCheck(IDbContextFactory<TContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _contextName = typeof(TContext).Name;
        }

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
                return new HealthCheckResult(context.Registration.FailureStatus, $"Cannot get hte list of pending migrations for {_contextName}.", e);
            }

            return HealthCheckResult.Healthy($"No pending migrations for {_contextName}.");
        }
    }
}