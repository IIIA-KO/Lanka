using Lanka.Modules.Users.Domain.Users;
using Lanka.Modules.Users.Domain.Users.BirthDates;
using Lanka.Modules.Users.Domain.Users.Emails;
using Lanka.Modules.Users.Domain.Users.FirstNames;
using Lanka.Modules.Users.Domain.Users.LastNames;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Users.Infrastructure.Users;

internal sealed class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder
            .HasIndex(user => user.Email)
            .IsUnique();

        builder
            .HasIndex(user => user.IdentityId)
            .IsUnique();

        builder
            .Property(user => user.Id)
            .HasConversion(id => id.Value, id => new UserId(id))
            .IsRequired();

        builder
            .Property(user => user.FirstName)
            .HasMaxLength(FirstName.MaxLength)
            .HasConversion(firstName => firstName.Value, value => FirstName.Create(value).Value)
            .IsRequired();

        builder
            .Property(user => user.LastName)
            .HasMaxLength(LastName.MaxLength)
            .HasConversion(lastName => lastName.Value, value => LastName.Create(value).Value)
            .IsRequired();

        builder
            .Property(user => user.Email)
            .HasMaxLength(Email.MaxLength)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .IsRequired();

        builder
            .Property(user => user.BirthDate)
            .HasConversion(
                birthDate => birthDate.Value,
                value => BirthDate.Create(value).Value)
            .IsRequired();
    }
}
