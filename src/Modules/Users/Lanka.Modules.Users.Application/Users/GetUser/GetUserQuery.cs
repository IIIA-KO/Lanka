using Lanka.Common.Application.Messaging;
using Lanka.Modules.Users.Domain.Users;

namespace Lanka.Modules.Users.Application.Users.GetUser
{
    public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
}
