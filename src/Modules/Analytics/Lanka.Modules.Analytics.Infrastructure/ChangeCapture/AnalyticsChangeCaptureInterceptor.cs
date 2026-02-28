using Lanka.Common.Domain;
using Lanka.Common.Infrastructure.ChangeCapture;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Infrastructure.ChangeCapture;

internal sealed class AnalyticsChangeCaptureInterceptor : ChangeCaptureInterceptorBase
{
    protected override CapturedChangeData? ExtractCapturedData(IChangeCaptured entity)
    {
        return entity switch
        {
            InstagramAccount a => new CapturedChangeData(
                nameof(InstagramAccount),
                a.Metadata.UserName,
                $"Instagram account @{a.Metadata.UserName}",
                ["instagram", "account"],
                new Dictionary<string, object>
                {
                    { "UserName", a.Metadata.UserName },
                    { "FollowersCount", a.Metadata.FollowersCount },
                    { "MediaCount", a.Metadata.MediaCount },
                }),

            _ => null,
        };
    }

    protected override string? GetItemType(IChangeCaptured entity)
    {
        return entity switch
        {
            InstagramAccount => nameof(InstagramAccount),
            _ => null,
        };
    }
}
