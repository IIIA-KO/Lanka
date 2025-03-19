using Lanka.Common.Application.Authentication;
using Microsoft.AspNetCore.Http;

namespace Lanka.Common.Infrastructure.Authentication
{
    public class UserContext : IUserContext
    {
        private readonly HttpContextAccessor _httpContextAccessor;

        public UserContext(HttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            return this._httpContextAccessor.HttpContext?.User.GetUserId()
                   ?? throw new InvalidOperationException("User context is unavailable");
        }

        public string GetIdentityId()
        {
            return this._httpContextAccessor.HttpContext?.User.GetIdentityId()
                   ?? throw new InvalidOperationException("User context is unavailable");
        }

        public string? AccessToken =>
            this._httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()
                ?.Split(' ')
                .LastOrDefault();
    }
}
