using Lanka.Common.Presentation.Endpoints;

namespace Lanka.Modules.Users.Presentation;

public abstract class UsersEndpointBase : EndpointBase
{
    protected override string BaseRoute => "users";
}