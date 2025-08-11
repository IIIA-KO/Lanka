using Lanka.Modules.Users.Infrastructure.Identity.Models;
using Refit;

namespace Lanka.Modules.Users.Infrastructure.Identity.Apis;

internal interface IKeycloakUserApi
{
    [Post("/users")]
    Task<HttpResponseMessage> CreateUserAsync(
        [Body] UserRepresentation user
    );
}
