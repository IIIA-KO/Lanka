using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Campaigns.Application.Instagram;

public sealed record UpdateInstagramDataCommand(Guid UserId, string Username, int FollowersCount, int MediaCount) : ICommand;
