using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Reviews.Delete;

public sealed record DeleteReviewCommand(Guid ReviewId) : ICommand;
