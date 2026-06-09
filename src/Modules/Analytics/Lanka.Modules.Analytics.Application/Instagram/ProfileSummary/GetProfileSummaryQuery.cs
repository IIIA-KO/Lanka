using Lanka.Common.Application.Messaging;
using Lanka.Modules.Analytics.Domain;

namespace Lanka.Modules.Analytics.Application.Instagram.ProfileSummary;

public sealed record GetProfileSummaryQuery(
    Guid UserId,
    StatisticsPeriod StatisticsPeriod
) : IQuery<ProfileSummaryResponse>;
