using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Refit;

namespace Lanka.Modules.Users.Infrastructure.Identity.Apis;

internal interface IKeycloakTokenApi
{
    [Post("/")]
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    Task<AuthorizationToken> GetTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)]
        FormUrlEncodedContent content
    );
}
