namespace Lanka.Modules.Users.Application.Abstractions.Data;

public interface IUserDeletionGuard
{
    Task<bool> HasActiveCampaignsAsync(Guid userId, CancellationToken cancellationToken = default);
}
