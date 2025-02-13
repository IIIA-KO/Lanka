using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Pacts
{
    public sealed record PactId(Guid Value) : TypedEntityId(Value)
    {
        public static PactId New() => new(Guid.NewGuid());
    }
}
