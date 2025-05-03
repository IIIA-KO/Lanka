using Lanka.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lanka.Modules.Users.Infrastructure.Users;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Code);

        builder.Property(p => p.Code).HasMaxLength(100);

        builder.HasData(
            Permission.ReadUsers,
            Permission.CreateUser,
            Permission.UpdateUser,
            Permission.DeleteUser,
            Permission.ReadProfile,
            Permission.UpdateProfile,
            Permission.ReadBloggers,
            Permission.ReadOffers,
            Permission.CreateOffer,
            Permission.UpdateOffer,
            Permission.DeleteOffer,
            Permission.ReadPacts,
            Permission.CreatePact,
            Permission.UpdatePact,
            Permission.ReadCampaigns,
            Permission.CreateCampaign,
            Permission.UpdateCampaign,
            Permission.ReadReviews,
            Permission.CreateReview,
            Permission.UpdateReview,
            Permission.DeleteReview
        );

        builder
            .HasMany<Role>()
            .WithMany()
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable("role_permissions");

                joinBuilder.HasData(
                    // Member permissions
                    CreateRolePermission(Role.Member, Permission.ReadProfile),
                    CreateRolePermission(Role.Member, Permission.UpdateProfile),
                    CreateRolePermission(Role.Member, Permission.ReadBloggers),
                    CreateRolePermission(Role.Member, Permission.CreatePact),
                    CreateRolePermission(Role.Member, Permission.ReadPacts),
                    CreateRolePermission(Role.Member, Permission.UpdatePact),
                    CreateRolePermission(Role.Member, Permission.CreateCampaign),
                    CreateRolePermission(Role.Member, Permission.ReadCampaigns),
                    CreateRolePermission(Role.Member, Permission.UpdateCampaign),
                    CreateRolePermission(Role.Member, Permission.CreateReview),
                    CreateRolePermission(Role.Member, Permission.ReadReviews),
                    CreateRolePermission(Role.Member, Permission.UpdateReview),
                    CreateRolePermission(Role.Member, Permission.DeleteReview),

                    // Admin permissions
                    CreateRolePermission(Role.Administrator, Permission.ReadProfile),
                    CreateRolePermission(Role.Administrator, Permission.UpdateProfile),
                    CreateRolePermission(Role.Administrator, Permission.ReadUsers),
                    CreateRolePermission(Role.Administrator, Permission.CreateUser),
                    CreateRolePermission(Role.Administrator, Permission.UpdateUser),
                    CreateRolePermission(Role.Administrator, Permission.DeleteUser),
                    CreateRolePermission(Role.Administrator, Permission.ReadBloggers),
                    CreateRolePermission(Role.Administrator, Permission.ReadPacts),
                    CreateRolePermission(Role.Administrator, Permission.UpdatePact),
                    CreateRolePermission(Role.Administrator, Permission.ReadCampaigns),
                    CreateRolePermission(Role.Administrator, Permission.ReadReviews),
                    CreateRolePermission(Role.Administrator, Permission.UpdateReview),
                    CreateRolePermission(Role.Administrator, Permission.DeleteReview)
                );
            });
    }

    private static object CreateRolePermission(Role role, Permission permission)
    {
        return new
        {
            RoleName = role.Name,
            PermissionCode = permission.Code
        };
    }
}
