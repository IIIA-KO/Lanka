using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Pacts.Create;

public sealed record CreatePactCommand(string Content) : ICommand<Guid>;
