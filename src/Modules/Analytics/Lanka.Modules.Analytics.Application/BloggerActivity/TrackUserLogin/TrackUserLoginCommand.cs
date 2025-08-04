using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Analytics.Application.BloggerActivity.TrackUserLogin;

public record TrackUserLoginCommand(Guid UserId, DateTimeOffset LastLoggedInAtUtc) : ICommand;
