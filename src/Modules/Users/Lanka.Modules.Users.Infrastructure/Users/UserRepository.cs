using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Users.Infrastructure.Users;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;

    public UserRepository(UsersDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await this._dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public void Add(User user)
    {
        foreach (Role role in user.Roles)
        {
            this._dbContext.Attach(role);
        }

        this._dbContext.Users.Add(user);
    }

    public void Remove(User user)
    {
        user.Deleted();
        this._dbContext.Users.Remove(user);
    }
}
