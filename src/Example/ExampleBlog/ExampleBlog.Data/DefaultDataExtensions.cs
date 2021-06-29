using RESTworld.EntityFrameworkCore.Models;
using System;

namespace ExampleBlog.Data
{
    /// <summary>
    /// This class just exists to make it easier to create some example data.
    /// </summary>
    internal static class DefaultDataExtensions
    {
        static DateTimeOffset creationDate = new DateTimeOffset(2021, 1, 2, 3, 4, 5, TimeSpan.FromHours(6));
        const string creator = "me";
        internal static T AddDefaults<T>(this T entity)
            where T : ChangeTrackingEntityBase
        {
            entity.CreatedAt = creationDate;
            entity.CreatedBy = creator;
            entity.LastChangedAt = creationDate;
            entity.LastChangedBy = creator;

            return entity;
        }
    }
}
