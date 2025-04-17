using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Pacts;

internal static class PactData
{
    public static Pact CreatePact()
    {
        Pact pact = Pact.Create(
            BloggerId,
            Content
        ).Value;

        return pact;
    }

    public static BloggerId BloggerId => BloggerId.New();
    
    public static string Content => "Test Content";
}
