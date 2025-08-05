using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackUserLogin;

public sealed record TrackUserLoginCommand(Guid UserId, DateTimeOffset LastLoggedInAtUtc) : ICommand;
