using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;

namespace Lanka.Modules.Campaigns.Application.Pacts.Edit;

public sealed record EditPactCommand(string Content) : ICommand<PactResponse>;
