using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Users.Infrastructure.Users
{
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

        public void Add(User user)
        {
            this._dbContext.Users.Add(user);
        }
    }
}
