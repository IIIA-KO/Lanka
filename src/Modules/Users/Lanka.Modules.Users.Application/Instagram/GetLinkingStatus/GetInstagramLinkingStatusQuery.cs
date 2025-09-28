using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Application.Instagram.Models;

namespace Lanka.Modules.Users.Application.Instagram.GetLinkingStatus;

public sealed record GetInstagramLinkingStatusQuery : IQuery<InstagramOperationStatus?>;
