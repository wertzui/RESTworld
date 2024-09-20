using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query
{
#pragma warning disable EF1001 // Internal EF Core API usage.
    /// <summary>
    /// Fixes FromTo and Between temporal queries so they can be joined with non-temporal entities.
    /// See https://github.com/dotnet/efcore/issues/27259
    /// </summary>
    public class AllowAllTemporalSqlServerNavigationExpansionExtensibilityHelper : SqlServerNavigationExpansionExtensibilityHelper, INavigationExpansionExtensibilityHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllowAllTemporalSqlServerNavigationExpansionExtensibilityHelper"/> class.
        /// </summary>
        /// <param name="dependencies"></param>
        public AllowAllTemporalSqlServerNavigationExpansionExtensibilityHelper(NavigationExpansionExtensibilityHelperDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc/>
        public override EntityQueryRootExpression CreateQueryRoot(IEntityType entityType, EntityQueryRootExpression? source)
        {
            //BEGIN CUSTOM CODE
            if (source is TemporalQueryRootExpression
                && !entityType.IsMappedToJson()
                && !OwnedEntityMappedToSameTableAsOwner(entityType))
            {
                if (!entityType.GetRootType().IsTemporal())
                {
                    //Expand the non-temporal type with a default EntityQueryRootExpression
                    return source.QueryProvider != null ? new EntityQueryRootExpression(source.QueryProvider, entityType) : new EntityQueryRootExpression(entityType);
                }
            }
            //END CUSTOM CODE

            return base.CreateQueryRoot(entityType, source);
        }

        /// <inheritdoc/>
        public override void ValidateQueryRootCreation(IEntityType entityType, EntityQueryRootExpression? source)
        {
            if (source is TemporalQueryRootExpression
                && !entityType.IsMappedToJson()
                && !OwnedEntityMappedToSameTableAsOwner(entityType))
            {
                if (!entityType.GetRootType().IsTemporal())
                {
                    //BEGIN CUSTOM CODE
                    //Allow temporal queries 'AsOf' and 'FromTo' to navigate to non temporal types
                    return;
                    //END CUSTOM CODE
                }

                if (source is not TemporalAsOfQueryRootExpression)
                    throw new InvalidOperationException(SqlServerStrings.TemporalNavigationExpansionOnlySupportedForAsOf("AsOf"));
            }

            base.ValidateQueryRootCreation(entityType, source);
        }

        private bool OwnedEntityMappedToSameTableAsOwner(IEntityType entityType)
            => entityType.IsOwned()
                && entityType.FindOwnership()!.PrincipalEntityType.GetTableMappings().FirstOrDefault()?.Table is ITable ownerTable
                    && entityType.GetTableMappings().FirstOrDefault()?.Table is ITable entityTable
                    && ownerTable == entityTable;
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}