using Lanka.Modules.Users.Application.Abstractions.Data;
using Lanka.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Lanka.Modules.Users.Infrastructure.Database
{
    public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options), IUnitOfWork
    {
        internal DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Users);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
