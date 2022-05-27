using Microsoft.Extensions.DependencyInjection;
using RESTworld.Business.Authorization.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace RESTworld.Testing
{
    /// <summary>
    /// Contains extension methods related to <see cref="UserTestConfiguration"/>.
    /// </summary>
    public static class UserTestBuilderExtensions
    {
        /// <summary>
        /// Adds an "empty" user to your tests. You can use this if you do not want to check for
        /// user authorization in your tests, but need an <see cref="IUserAccessor"/>.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithoutUser(this ITestBuilder builder)
        {
            return builder.With(new UserTestConfiguration(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        /// <summary>
        /// Adds a user to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="name">
        /// The name of the user or null. If no name is provided, "TestName" is used.
        /// </param>
        /// <param name="authenticationType">
        /// The authentication type or null. If no authentication type is provided,
        /// "TestAuthentication" is used.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithUser(this ITestBuilder builder, string? name = null, string? authenticationType = null)
        {
            return builder.With(new UserTestConfiguration(name, authenticationType));
        }

        /// <summary>
        /// Adds a user to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="claims">
        /// The claims to add to the user or null. If no claims are given, a user without any claims
        /// is created.
        /// </param>
        /// <param name="authenticationType">
        /// The authentication type or null. If no authentication type is provided,
        /// "TestAuthentication" is used.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithUser(this ITestBuilder builder, IEnumerable<Claim>? claims, string? authenticationType)
        {
            return builder.With(new UserTestConfiguration(claims, authenticationType));
        }

        /// <summary>
        /// Adds a user to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="claimsIdentity">The claims identity that represents the user.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithUser(this ITestBuilder builder, ClaimsIdentity claimsIdentity)
        {
            return builder.With(new UserTestConfiguration(claimsIdentity));
        }

        /// <summary>
        /// Adds a user to your tests.
        /// </summary>
        /// <param name="builder">The builder to add the configuration to.</param>
        /// <param name="claimsPrincipal">The claims principal that represents the user.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static ITestBuilder WithUser(this ITestBuilder builder, ClaimsPrincipal claimsPrincipal)
        {
            return builder.With(new UserTestConfiguration(claimsPrincipal));
        }
    }

    /// <summary>
    /// This test configuration can add a user with an <see cref="IUserAccessor"/> to your tests.
    /// </summary>
    public class UserTestConfiguration : ITestConfiguration
    {
        private readonly IUserAccessor _userAccessor;

        /// <summary>
        /// Creates a new instance of the <see cref="UserTestConfiguration"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the user or null. If no name is provided, "TestName" is used.
        /// </param>
        /// <param name="authenticationType">
        /// The authentication type or null. If no authentication type is provided,
        /// "TestAuthentication" is used.
        /// </param>
        public UserTestConfiguration(string? name = null, string? authenticationType = null)
            : this(new[] { new Claim(ClaimTypes.Name, name ?? "TestName") }, authenticationType ?? "TestAuthentication")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserTestConfiguration"/> class.
        /// </summary>
        /// <param name="claims">
        /// The claims to add to the user or null. If no claims are given, a user without any claims
        /// is created.
        /// </param>
        /// <param name="authenticationType">
        /// The authentication type or null. If no authentication type is provided,
        /// "TestAuthentication" is used.
        /// </param>
        public UserTestConfiguration(IEnumerable<Claim>? claims, string? authenticationType = null)
            : this(new ClaimsIdentity(claims, authenticationType ?? "TestAuthentication"))
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserTestConfiguration"/> class.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity that represents the user.</param>
        /// <exception cref="ArgumentNullException"><paramref name="claimsIdentity"/></exception>
        public UserTestConfiguration(ClaimsIdentity claimsIdentity)
            : this(new ClaimsPrincipal(claimsIdentity))
        {
            if (claimsIdentity is null)
                throw new ArgumentNullException(nameof(claimsIdentity));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserTestConfiguration"/> class.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal that represents the user.</param>
        /// <exception cref="ArgumentNullException"><paramref name="claimsPrincipal"/></exception>
        public UserTestConfiguration(ClaimsPrincipal claimsPrincipal)
            : this(new TestUserAccessor(claimsPrincipal))
        {
            if (claimsPrincipal is null)
                throw new ArgumentNullException(nameof(claimsPrincipal));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserTestConfiguration"/> class.
        /// </summary>
        /// <param name="userAccessor">The user accessor that will return the user.</param>
        /// <exception cref="ArgumentNullException"><paramref name="userAccessor"/></exception>
        public UserTestConfiguration(IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
        }

        /// <inheritdoc/>
        public void AfterConfigureServices(IServiceProvider provider)
        {
        }

        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_userAccessor);
        }

        /// <summary>
        /// An <see cref="IUserAccessor"/> that will always return the given user.
        /// </summary>
        public class TestUserAccessor : IUserAccessor
        {
            /// <summary>
            /// Creates a new instance of the <see cref="TestUserAccessor"/> class.
            /// </summary>
            /// <param name="user">The user to return.</param>
            public TestUserAccessor(ClaimsPrincipal user)
            {
                User = user;
            }

            /// <inheritdoc/>
            public ClaimsPrincipal? User { get; }
        }
    }
}