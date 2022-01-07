using Microsoft.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.EntityFrameworkCore
{
    /// <summary>
    /// A base class for all your DbCOntext classes.
    /// It automatically adds timestamp concurrency detection for all <see cref="EntityBase"/> entities and
    /// Automatically modifies the LastChangedAt and LastChangedBy properties of <see cref="ChangeTrackingEntityBase"/> entities.
    /// It also removes the timeout during migrations so long running migrations won't fail.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class DbContextBase : DbContext
    {
        // This property indicates whether or not you're running inside LINQPad:
        private static bool _insideLINQPad = AppDomain.CurrentDomain.FriendlyName.StartsWith("LINQPad");

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextBase"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public DbContextBase(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Migrates the database to the latest version.
        /// </summary>
        public virtual void Migrate()
        {
            // Temporarily increase command timeout to prevent timeouts during migration.
            var oldTimeout = Database.GetCommandTimeout();
            Database.SetCommandTimeout(int.MaxValue);

            Database.Migrate();

            Database.SetCommandTimeout(oldTimeout);
        }

        /// <inheritdoc/>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            using (new TimestampConcurrencyDetection(ChangeTracker))
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// Also automatically adds values for entities that inherit from <see cref="ChangeTrackingEntityBase"/>.
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database.
        /// This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        /// Multiple active operations on the same context instance are not supported. Use
        /// 'await' to ensure that any asynchronous operations have completed before calling
        /// another method on this context.
        /// </summary>
        /// <param name="currentUser">The current user. The user will be written to the LastChangeBy and probably the CreatedBy columns.</param>
        /// <param name="cancellationToken">
        /// A System.Threading.CancellationToken to observe while waiting for the task to
        /// complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// the number of state entries written to the database.
        /// </returns>
        public Task<int> SaveChangesAsync(string? currentUser = null, CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(true, currentUser, cancellationToken);
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// Also automatically adds values for entities that inherit from <see cref="ChangeTrackingEntityBase"/>.
        /// This method will automatically call <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges"/>
        /// to discover any changes to entity instances before saving to the underlying database.
        /// This can be disabled via <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled"/>.
        /// Multiple active operations on the same context instance are not supported. Use
        /// 'await' to ensure that any asynchronous operations have completed before calling
        /// another method on this context.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        /// Indicates whether <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges"/>
        /// is called after the changes have been sent successfully to the database.
        /// </param>
        /// <param name="currentUser">The current user. The user will be written to the <see cref="ChangeTrackingEntityBase.LastChangedBy"/> and probably the <see cref="ChangeTrackingEntityBase.CreatedBy"/> columns.</param>
        /// <param name="cancellationToken">
        /// A System.Threading.CancellationToken to observe while waiting for the task to
        /// complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// the number of state entries written to the database.
        /// </returns>
        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, string? currentUser = null, CancellationToken cancellationToken = default)
        {
            AddChangedFields(currentUser);
            return SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_insideLINQPad)
            {
                optionsBuilder.UseLazyLoadingProxies();
                optionsBuilder.EnableSensitiveDataLogging(true);
            }

            base.OnConfiguring(optionsBuilder);
        }

        private void AddChangedFields(string? currentUser = null)
        {
            var now = DateTimeOffset.Now;
            var entities = ChangeTracker.Entries<ChangeTrackingEntityBase>();
            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Modified)
                {
                    entity.Entity.LastChangedAt = now;
                    entity.Entity.LastChangedBy = currentUser;

                    // Ensure CreatedAt and CreatedBy are never modified
                    var createdAtProperty = entity.Property(e => e.CreatedAt);
                    if (createdAtProperty.IsModified)
                    {
                        createdAtProperty.CurrentValue = createdAtProperty.OriginalValue;
                        createdAtProperty.IsModified = false;
                    }
                    var createdByProperty = entity.Property(e => e.CreatedBy);
                    if (createdByProperty.IsModified)
                    {
                        createdByProperty.CurrentValue = createdByProperty.OriginalValue;
                        createdByProperty.IsModified = false;
                    }
                }
                else if (entity.State == EntityState.Added)
                {
                    entity.Entity.LastChangedAt = now;
                    entity.Entity.LastChangedBy = currentUser;
                    entity.Entity.CreatedAt = now;
                    entity.Entity.CreatedBy = currentUser;
                }
            }
        }
    }
}