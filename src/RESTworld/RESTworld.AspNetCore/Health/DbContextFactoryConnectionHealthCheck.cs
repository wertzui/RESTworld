using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health
{
    public class DbContextFactoryConnectionHealthCheck<TContext> : IHealthCheck
        where TContext : DbContext
    {
        private readonly string _contextName;
        private readonly IDbContextFactory<TContext> _factory;

        public DbContextFactoryConnectionHealthCheck(IDbContextFactory<TContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _contextName = typeof(TContext).Name;
        }

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
}