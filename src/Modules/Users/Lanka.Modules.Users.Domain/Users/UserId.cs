using Lanka.Common.Domain;

namespace Lanka.Modules.Users.Domain.Users;

public record UserId(Guid Value) : TypedEntityId(Value)
{
    public static UserId New() => new(Guid.NewGuid());
}