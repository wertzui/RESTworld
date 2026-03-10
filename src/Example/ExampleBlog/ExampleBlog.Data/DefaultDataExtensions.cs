using RESTworld.EntityFrameworkCore.Models;
using System;

namespace ExampleBlog.Data;

/// <summary>
/// This class just exists to make it easier to create some example data.
/// </summary>
internal static class DefaultDataExtensions
{
    private const string _creator = "me";
    private static readonly DateTimeOffset _creationDate = new(2021, 1, 2, 3, 4, 5, TimeSpan.FromHours(6));

    internal static T AddDefaults<T>(this T entity)
        where T : ChangeTrackingEntityBase
    {
        entity.CreatedAt = _creationDate;
        entity.CreatedBy = _creator;
        entity.LastChangedAt = _creationDate;
        entity.LastChangedBy = _creator;

        return entity;
    }
}