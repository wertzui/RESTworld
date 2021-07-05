using Microsoft.AspNetCore.Http;
using RESTworld.Business.Abstractions;
using System;
using System.Security.Claims;

namespace RESTworld.AspNetCore.Authorization
{
    /// <inheritdoc/>
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Creates a new instance of the <see cref="UserAccessor"/> class.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc/>
        public ClaimsPrincipal User => _httpContextAccessor.HttpContext.User;
    }
}