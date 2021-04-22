using Microsoft.AspNetCore.Http;
using RESTworld.Business.Abstractions;
using System;
using System.Security.Claims;

namespace RESTworld.AspNetCore.Authorization
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ClaimsPrincipal User => _httpContextAccessor.HttpContext.User;
    }
}