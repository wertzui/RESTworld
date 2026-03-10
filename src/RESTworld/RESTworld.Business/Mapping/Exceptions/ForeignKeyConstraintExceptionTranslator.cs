using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RESTworld.Business.Models;
using RESTworld.Business.Validation;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RESTworld.Business.Mapping;

/// <summary>
/// Translates database foreign key constraint violations into user-friendly service responses for a specific entity and
/// DTO type.
/// </summary>
/// <remarks>
/// This translator detects foreign key constraint violations, typically resulting from attempts to
/// reference non-existent related entities, and converts them into structured validation or problem responses. It uses
/// the provided mapping and context factory to identify the relevant property on the DTO and generate meaningful error
/// messages.
/// </remarks>
/// <typeparam name="TContext">The type of the Entity Framework database context.</typeparam>
/// <typeparam name="TEntity">The entity type associated with the foreign key constraint.</typeparam>
/// <typeparam name="TGetFullDto">The data transfer object (DTO) type used for service responses.</typeparam>
public partial class ForeignKeyConstraintExceptionTranslator<TContext, TEntity, TGetFullDto> : ExceptionTranslator<TEntity, TGetFullDto>
    where TContext : DbContext
{
    [GeneratedRegex("FOREIGN KEY constraint \"(?<ForeignKeyName>(FK_(?<ForeignTable>\\w+)_(?<PrimaryTable>\\w+)_(?<ForeignKeyProperty>\\w+))|[^\"]+)\"")]
    private static partial Regex ForeignKeyRegex { get; }

    private readonly IMappingMemberNameProvider<TEntity, TGetFullDto> _memberNameProvider;
    private readonly IDbContextFactory<TContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the ForeignKeyConstraintExceptionTranslator class with the specified member name provider and database context factory.
    /// </summary>
    /// <param name="memberNameProvider">An implementation that provides mapping between entity member names and their corresponding DTO member names.</param>
    /// <param name="contextFactory">A factory used to create instances of the database context associated with the entity type.</param>
    /// <exception cref="ArgumentNullException">Thrown if memberNameProvider or contextFactory is null.</exception>
    public ForeignKeyConstraintExceptionTranslator(IMappingMemberNameProvider<TEntity, TGetFullDto> memberNameProvider, IDbContextFactory<TContext> contextFactory)
    {
        _memberNameProvider = memberNameProvider ?? throw new ArgumentNullException(nameof(memberNameProvider));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <inheritdoc/>
    public override bool TryTranslate(Exception exception, [NotNullWhen(true)] out ServiceResponse<TGetFullDto>? response)
    {
        // We only handle Foreign Key constraint violations, which are DbUpdateExceptions with an inner SqlException with number 547.
        if (exception is not DbUpdateException updateException || updateException.InnerException is not SqlException sqlException || sqlException.Number != 547)
        {
            response = null;
            return false;
        }

        if (sqlException.Message is null)
        {
            response = ServiceResponse.FromException<TGetFullDto>(sqlException);
            return true;
        }

        var match = ForeignKeyRegex.Match(sqlException.Message);
        if (!match.Success)
        {
            response = ServiceResponse.FromException<TGetFullDto>(sqlException);
            return true;
        }

        if (match.Groups.TryGetValue("ForeignKeyName", out var foreignKeyName))
        {
            // If we have a foreign key name we try to walk the whole path backwards from the entity that defines the key, to the dto, to the property on the dto that caused the failure.
            using var context = _contextFactory.CreateDbContext();
            var foreignKeyContraint = context.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()).FirstOrDefault(f => f.GetConstraintName() == foreignKeyName.Value);

            if (foreignKeyContraint is not null)
            {
                var entityType = foreignKeyContraint.PrincipalEntityType.ClrType;
                var returnType = typeof(TGetFullDto);
                var returnTypeProperties = returnType.GetProperties();

                var foreignKeyPropertyOnDto = returnTypeProperties
                    .Select(p => FindForeignKeyProperty(returnType, returnTypeProperties, p))
                    .Where(a => a is not null)
                    .FirstOrDefault();

                if (foreignKeyPropertyOnDto is not null)
                {
                    var validationResults = new ValidationResults(foreignKeyPropertyOnDto, $"Foreign key was violated.");
                    response = ServiceResponse.FromFailedValidation<TGetFullDto>(HttpStatusCode.Conflict, validationResults);
                    return true;
                }
            }

            // If that fails, we simply return a global failure.
            response = ServiceResponse.FromProblem<TGetFullDto>(HttpStatusCode.Conflict, $"Invalid relationship. The foreign key '{foreignKeyName}' was violated.");
        }

        if (match.Groups.TryGetValue("PrimaryTable", out var primaryTable))
        {
            response = ServiceResponse.FromProblem<TGetFullDto>(HttpStatusCode.Conflict, $"Invalid relationship. '{primaryTable}' was not found.");
            return true;
        }

        response = ServiceResponse.FromException<TGetFullDto>(HttpStatusCode.Conflict, sqlException);
        return true;
    }

    private static string? FindForeignKeyProperty(Type dtoType, PropertyInfo[] returnTypeProperties, PropertyInfo property)
    {
        // First look for a ForeignKeyAttribute on the property itself
        var foreignKeyAttributeFromProperty = property.PropertyType.GetCustomAttribute<ForeignKeyAttribute>();
        if (foreignKeyAttributeFromProperty is not null)
            return foreignKeyAttributeFromProperty.Name;

        // Then look for a ForeignKeyAttribute pointing to this property
        var propertyWithForeignKeyAttributeToForeignKeyProperty = returnTypeProperties.FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>()?.Name == property.Name);
        if (propertyWithForeignKeyAttributeToForeignKeyProperty is not null)
            return propertyWithForeignKeyAttributeToForeignKeyProperty.Name;

        // Lastly check the convention with the Id suffix
        var nameWithId = property.Name + "Id";
        var propertyWithIdSuffix = dtoType.GetProperty(nameWithId, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.IgnoreCase);

        // May be null if it is not found.
        return propertyWithIdSuffix?.Name;
    }
}
