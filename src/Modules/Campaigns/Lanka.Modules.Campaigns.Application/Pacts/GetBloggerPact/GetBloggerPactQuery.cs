using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;

public sealed record GetBloggerPactQuery(Guid BloggerId) : IQuery<PactResponse>;
