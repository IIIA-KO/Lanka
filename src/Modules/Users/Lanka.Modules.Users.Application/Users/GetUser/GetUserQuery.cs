using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
