using Lanka.Common.Application.Authorization;
using Lanka.Common.Application.Messaging;

namespace Lanka.Modules.Users.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
