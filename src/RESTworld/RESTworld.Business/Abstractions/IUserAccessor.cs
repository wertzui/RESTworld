using System.Security.Claims;

namespace RESTworld.Business.Abstractions
{
    public interface IUserAccessor
    {
        public ClaimsPrincipal User { get; }
    }
}