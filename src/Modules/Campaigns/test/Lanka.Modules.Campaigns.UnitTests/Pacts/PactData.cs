using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Pacts;

internal static class PactData
{
    public static BloggerId BloggerId => BloggerId.New();
    
    public static string Content => BaseTest.Faker.Lorem.Sentence();
}
