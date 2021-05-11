using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.HostedServices
{
    public class DatabaseMigrationHostedService<TDbContext> : BackgroundService
        where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _factory;
        private readonly ILogger<DatabaseMigrationHostedService<TDbContext>> _logger;
        private const int MIGRATION_TIMEOUT = int.MaxValue;

        public DatabaseMigrationHostedService(
            IDbContextFactory<TDbContext> factory,
            ILogger<DatabaseMigrationHostedService<TDbContext>> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
                    if(timeout != MIGRATION_TIMEOUT)
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
}