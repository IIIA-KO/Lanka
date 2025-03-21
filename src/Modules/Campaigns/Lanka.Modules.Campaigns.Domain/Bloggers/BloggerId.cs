using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public sealed record BloggerId(Guid Value) : TypedEntityId(Value)
{
    public static BloggerId New() => new (Guid.NewGuid());
}
