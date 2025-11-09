using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RESTworld.EntityFrameworkCore;

/// <summary>
/// Use the time stamp concurrency detection around the SaveChanges or SaveChangesAsync methods of a DbContext
/// to apply concurrency detection using the time stamp in an environment with disconnected entities.
/// </summary>
/// <seealso cref="System.IDisposable" />
public class TimestampConcurrencyDetection : IDisposable
{
    private readonly IDictionary<PropertyEntry<ConcurrentEntityBase, byte[]?>, byte[]?> _modifiedEntries = new Dictionary<PropertyEntry<ConcurrentEntityBase, byte[]?>, byte[]?>();

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
        var entities = changeTracker.Entries<ConcurrentEntityBase>();

        foreach (var entity in entities)
        {
            if (entity.State is EntityState.Modified or EntityState.Deleted)
            {
                // entity.Property(e => e.Timestamp) throws if the Timestamp has a [NotMapped] Attribute.
                var timestampProperty = GetPropertyOrDefault(entity, e => e.Timestamp);
                if (timestampProperty is not null)
                {
                    _modifiedEntries.Add(timestampProperty, timestampProperty.OriginalValue);
                    timestampProperty.OriginalValue = timestampProperty.CurrentValue;
                }
            }
        }
    }

    private static PropertyEntry<TEntity, TProperty>? GetPropertyOrDefault<TEntity, TProperty>(EntityEntry<TEntity> entityEntry, Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : class
    {
        var name = GetSimpleMemberName(propertyExpression.GetMemberAccess());
        var property = entityEntry.Properties.FirstOrDefault(p => p.Metadata.Name == name) as PropertyEntry<TEntity, TProperty>;
        return property;
    }

    private static string GetSimpleMemberName(MemberInfo member)
    {
        var name = member.Name;
        var num = name.LastIndexOf('.');
        if (num < 0)
        {
            return name;
        }

        return name[(num + 1)..];
    }
}