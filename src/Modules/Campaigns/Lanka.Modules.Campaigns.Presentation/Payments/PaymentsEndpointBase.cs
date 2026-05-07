using Lanka.Common.Presentation.Endpoints;

namespace Lanka.Modules.Campaigns.Presentation.Payments;

internal abstract class PaymentsEndpointBase : EndpointBase
{
    protected override string BaseRoute => "campaigns";
}
