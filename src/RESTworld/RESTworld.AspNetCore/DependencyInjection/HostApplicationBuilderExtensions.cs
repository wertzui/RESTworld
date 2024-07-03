using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Services.Abstractions;
using RESTworld.EntityFrameworkCore.Models;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class HostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a custom service with basic authorization, using a custom <typeparamref
    /// name="TService"/> and the <typeparamref name="TAuthorizationHandler"/>. Note that you
    /// need to write the custom controller which will call your service yourself and put it
    /// into the /Controllers folder of your application so it gets automatically recognized.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCustomServiceAndAuthorization<TRequest, TResponse, TService, TAuthorizationHandler>(this IHostApplicationBuilder builder)
        where TService : class
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddCustomServiceAndAuthorization<TRequest, TResponse, TService, TAuthorizationHandler>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds a custom service with basic authorization, using a custom <typeparamref
    /// name="TService"/> and the <typeparamref name="TAuthorizationHandler"/>. Note that you
    /// need to write the custom controller which will call your service yourself and put it
    /// into the /Controllers folder of your application so it gets automatically recognized.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TService">
    /// The type of the custom <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/> implementation.
    /// </typeparam>
    /// <typeparam name="TAuthorizationHandler">
    /// The type of the <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto,
    /// TGetFullDto, TUpdateDto}"/>.
    /// </typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddCustomServiceAndAuthorization<TEntity, TRequest, TResponse, TService, TAuthorizationHandler>(this IHostApplicationBuilder builder)
        where TEntity : ConcurrentEntityBase
        where TService : class
        where TAuthorizationHandler : class, IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddCustomServiceAndAuthorization<TEntity, TRequest, TResponse, TService, TAuthorizationHandler>(builder.Configuration);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds a pooled <see cref="IDbContextFactory{TContext}"/> to the service collection. The
    /// connection string comes from the configuration section "ConnectionStrings" with the name
    /// of the context type.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="optionsAction">An optional custom configuration. Is none is supplied, these are applied by default: <see cref="SqlServerDbContextOptionsExtensions.UseSqlServer(DbContextOptionsBuilder, Action{SqlServerDbContextOptionsBuilder}?)"/>, <see cref="DbContextOptionsBuilder.EnableDetailedErrors(bool)"/>, <see cref="DbContextOptionsBuilder.EnableServiceProviderCaching(bool)"/></param>
    /// <param name="sqlServerOptionsAction">An optional custom configuration for the SQL server connection. If none is supplied, <see cref="SqlServerDbContextOptionsBuilder.EnableRetryOnFailure()"/> is added as default. If an <paramref name="optionsAction"/> is provided, that this is not called by default.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddDbContextFactoryWithDefaults<TContext>(
        this IHostApplicationBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
                where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddDbContextFactoryWithDefaults<TContext>(builder.Configuration, optionsAction, sqlServerOptionsAction);
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds a factory to generate foreign keys to the CRUD controller endpoint of the given
    /// List DTO.
    /// </summary>
    /// <typeparam name="TListDto">The type of the List DTO.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddForeignKeyForFormTo<TListDto>(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddForeignKeyForFormTo<TListDto>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds and <see cref="IUserAccessor"/> to the services which can be used to retrieve the
    /// current user from the HttpContext.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IHostApplicationBuilder AddUserAccessor(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddUserAccessor();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }

    /// <summary>
    /// Adds the database to the list of databases to migrate to the latest version during startup.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="builder">The host application builder.</param>
    public static IHostApplicationBuilder MigrateDatabaseDuringStartup<TDbContext>(this IHostApplicationBuilder builder)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.MigrateDatabaseDuringStartup<TDbContext>();
#pragma warning restore CS0618 // Type or member is obsolete

        return builder;
    }
}