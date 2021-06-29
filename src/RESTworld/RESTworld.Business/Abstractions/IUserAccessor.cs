using System.Security.Claims;

namespace RESTworld.Business.Abstractions
{
    /// <summary>
    /// An abstraction to get the current user.
    /// This is normally retrieved from the HttpContext.
    /// </summary>
    public interface IUserAccessor
    {
        /// <summary>
        /// The current user.
        /// </summary>
        public ClaimsPrincipal User { get; }
    }
}