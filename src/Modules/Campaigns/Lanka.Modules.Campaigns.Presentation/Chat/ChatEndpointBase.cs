using Lanka.Common.Presentation.Endpoints;

namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal abstract class ChatEndpointBase : EndpointBase
{
    protected override string BaseRoute => "chats";
}
