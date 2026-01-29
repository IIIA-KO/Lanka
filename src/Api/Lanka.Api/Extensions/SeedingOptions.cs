namespace Lanka.Api.Extensions;

internal sealed class SeedingOptions
{
    public bool Enabled { get; init; } = true;

    public int FakeUserCount { get; init; } = 20;

    public int FakeCampaignsPerBlogger { get; init; } = 3;
}
