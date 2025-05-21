using Lanka.Modules.Users.Domain.Users.Emails;

namespace Lanka.Modules.Users.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    void Add(User user);
    
    void Remove(User user);
}
