using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Pacts;

public static class PactErrors
{
    public static readonly Error Duplicate =
        Error.Conflict("Pact.Duplicate", "Pact for specified blogger already exists.");

    public static readonly Error NotFound =
        Error.NotFound("Pact.NotFound", "Pact with specified identifier was not found.");
}
