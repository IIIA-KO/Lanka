using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Pacts.Contents;

public static class ContentErrors
{
    public static readonly Error Empty =
        Error.Validation("Content.Empty", "Content cannot empty");
}
