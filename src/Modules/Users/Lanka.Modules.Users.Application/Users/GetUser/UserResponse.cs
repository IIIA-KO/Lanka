using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.GetUser;

public sealed class UserResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public DateOnly BirthDay { get; init; }

    public static UserResponse FromUser(User user)
    {
        return new UserResponse
        {
            Id = user.Id.Value,
            FirstName = user.FirstName.Value,
            LastName = user.LastName.Value,
            Email = user.Email.Value,
            BirthDay = user.BirthDate.Value
        };
    }
}
