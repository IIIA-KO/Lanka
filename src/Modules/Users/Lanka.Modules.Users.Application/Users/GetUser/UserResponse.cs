namespace Lanka.Modules.Users.Application.Users.GetUser
{
    public sealed record UserResponse(Guid Id, string FirstName, string LastName, string Email, DateOnly Birthday);
}
