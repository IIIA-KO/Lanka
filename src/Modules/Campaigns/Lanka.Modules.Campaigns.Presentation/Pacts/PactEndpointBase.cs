using Lanka.Common.Presentation.Endpoints;

namespace Lanka.Modules.Campaigns.Presentation.Pacts;

public abstract class PactEndpointBase : EndpointBase
{
    protected override string BaseRoute => "pacts";
}
