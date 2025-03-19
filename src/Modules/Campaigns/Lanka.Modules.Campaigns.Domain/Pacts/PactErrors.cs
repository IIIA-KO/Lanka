using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Pacts
{
    public static class PactErrors
    {
        public static readonly Error AlreadyPublished = 
            Error.Conflict("Pact.AlreadyPublished", "Pact is already published.");
    }
}
