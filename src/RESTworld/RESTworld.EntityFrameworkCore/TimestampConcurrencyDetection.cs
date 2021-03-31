using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;

namespace RESTworld.EntityFrameworkCore
{
    /// <summary>
    /// Use the time stamp concurrency detection around the SaveChanges or SaveChangesAsync methods of a DbContext
    /// to apply concurrency detection using the time stamp in an environment with disconnected entities.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class TimestampConcurrencyDetection : IDisposable
    {
        private readonly IDictionary<PropertyEntry<EntityBase, byte[]>, byte[]> _modifiedEntries = new Dictionary<PropertyEntry<EntityBase, byte[]>, byte[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampConcurrencyDetection"/> class.
        /// </summary>
        /// <param name="changeTracker">The change tracker.</param>
        public TimestampConcurrencyDetection(ChangeTracker changeTracker)
        {
            SetOriginalTimestampValueForConcurrencyDetection(changeTracker);
        }

        /// <inheritdoc/>
        public void Dispose() => ResetTimestamps();

        private void ResetTimestamps()
        {
            foreach (var entry in _modifiedEntries)
            {
                entry.Key.OriginalValue = entry.Value;
            }
        }

        private void SetOriginalTimestampValueForConcurrencyDetection(ChangeTracker changeTracker)
        {
            var entities = changeTracker.Entries<EntityBase>();

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Modified || entity.State == EntityState.Deleted)
                {
                    var timestampProperty = entity.Property(e => e.Timestamp);
                    _modifiedEntries.Add(timestampProperty, timestampProperty.OriginalValue);
                    timestampProperty.OriginalValue = timestampProperty.CurrentValue;
                }
            }
        }
    }
}