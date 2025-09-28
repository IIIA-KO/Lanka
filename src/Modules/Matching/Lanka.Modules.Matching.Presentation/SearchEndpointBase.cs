using Lanka.Common.Presentation.Endpoints;

namespace Lanka.Modules.Matching.Presentation;

public abstract class SearchEndpointBase : EndpointBase
{
    protected override string BaseRoute => "search";
}
