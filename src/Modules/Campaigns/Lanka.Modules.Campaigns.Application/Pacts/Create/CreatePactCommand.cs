using Lanka.Common.Application.Messaging;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.Pacts.Create;

public sealed record CreatePactCommand(string Content) : ICommand<Guid>;
